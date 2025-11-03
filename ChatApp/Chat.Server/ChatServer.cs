// File: Chat.Server/ChatServer.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Chat.Shared;

namespace Chat.Server;

public class ConnectionState
{
    public TcpClient Client { get; }
    public NetworkStream Stream { get; }
    public string? Username { get; set; }

    public ConnectionState(TcpClient client)
    {
        Client = client;
        Stream = client.GetStream();
    }
}

public class ChatServer
{
    private readonly TcpListener _listener;

    // 🔹 Quản lý danh sách người dùng & phòng
    private readonly ConcurrentDictionary<string, ConnectionState> _users = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConnectionState>> _rooms = new();

    public ChatServer(string ip, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ip), port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"Server listening on {_listener.LocalEndpoint}");

        while (true)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                await Task.Delay(50); // tránh backlog socket khi test nhanh

                Console.WriteLine($"New client connected: {client.Client.RemoteEndPoint}");
                var connection = new ConnectionState(client);

                _ = HandleClientAsync(connection); // chạy client async
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting client: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(ConnectionState connection)
    {
        try
        {
            while (true)
            {
                BaseMessage message = await NetworkHelpers.ReadMessageAsync(connection.Stream);

                switch (message)
                {
                    // 🔸 Đăng nhập
                    case LoginMessage login:
                        connection.Username = login.Username;
                        _users[login.Username] = connection;
                        Console.WriteLine($"✅ {login.Username} logged in.");
                        break;

                    // 🔸 Chat công khai
                    case ChatPublicMessage chat:
                        Console.WriteLine($"[PUBLIC] {chat.From}: {chat.Text}");
                        break;

                    // 🔸 Chat riêng
                    case ChatPrivateMessage dm:
                        break;

                    // 🔹 Người 3: Tạo phòng
                    case CreateRoomMessage createRoom:
                        {
                            string room = createRoom.Room;
                            string creator = connection.Username ?? "unknown";

                            if (_rooms.ContainsKey(room))
                            {
                                var err = new ErrorMessage { Type = "error", Text = $"Room '{room}' already exists" };
                                await NetworkHelpers.SendMessageAsync(connection.Stream, err);
                                break;
                            }

                            _rooms[room] = new ConcurrentDictionary<string, ConnectionState>();
                            _rooms[room].TryAdd(creator, connection);

                            Console.WriteLine($"[ROOM] {creator} created room '{room}'");

                            var ok = new StatusMessage { Type = "status", Status = "ok", Text = $"Room '{room}' created" };
                            await NetworkHelpers.SendMessageAsync(connection.Stream, ok);

                            await BroadcastRoomListAsync();
                            break;
                        }

                    // 🔹 Người 3: Tham gia phòng
                    case JoinRoomMessage joinRoom:
                        {
                            string room = joinRoom.Room;
                            string user = connection.Username ?? "unknown";

                            if (!_rooms.TryGetValue(room, out var members))
                            {
                                var err = new ErrorMessage { Type = "error", Text = $"Room '{room}' not found" };
                                await NetworkHelpers.SendMessageAsync(connection.Stream, err);
                                break;
                            }

                            members[user] = connection;
                            Console.WriteLine($"[ROOM] {user} joined room '{room}'");

                            var ok = new StatusMessage { Type = "status", Status = "ok", Text = $"Joined room '{room}'" };
                            await NetworkHelpers.SendMessageAsync(connection.Stream, ok);
                            break;
                        }

                    // 🔹 Người 3: Chat trong phòng
                    case ChatRoomMessage chatRoom:
                        {
                            string room = chatRoom.Room;
                            string senderName = connection.Username ?? "unknown";

                            if (!_rooms.TryGetValue(room, out var members) || !members.ContainsKey(senderName))
                            {
                                var err = new ErrorMessage { Type = "error", Text = "You are not in this room" };
                                await NetworkHelpers.SendMessageAsync(connection.Stream, err);
                                break;
                            }

                            var msg = new ChatRoomMessage
                            {
                                Type = "chat_room",
                                Room = room,
                                From = senderName,
                                Text = chatRoom.Text
                            };

                            foreach (var kvp in members)
                            {
                                var targetConn = kvp.Value;
                                if (targetConn != connection)
                                    await NetworkHelpers.SendMessageAsync(targetConn.Stream, msg);
                            }

                            Console.WriteLine($"[ROOM] {senderName}@{room}: {chatRoom.Text}");
                            break;
                        }

                    // 🔸 Logout
                    case LogoutMessage logout:
                        throw new IOException("User requested logout.");

                    default:
                        Console.WriteLine($"Unknown message type: {message.Type}");
                        break;
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Client disconnected: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client: {ex.Message}");
        }
        finally
        {
            if (connection.Username != null)
            {
                _users.TryRemove(connection.Username, out _);

                // 🔹 Xóa user khỏi tất cả phòng
                foreach (var room in _rooms.Values)
                    room.TryRemove(connection.Username, out _);

                Console.WriteLine($"User {connection.Username} cleaned up.");
            }

            // 🔹 Đảm bảo giải phóng socket hoàn toàn
            try
            {
                connection.Stream.Close();
                connection.Client.Close();
            }
            catch { }
        }
    }

    // 🧩 Gửi tin nhắn đến toàn bộ user trong phòng
    private async Task BroadcastToRoomAsync(string room, BaseMessage message)
    {
        if (!_rooms.TryGetValue(room, out var members)) return;

        foreach (var kvp in members)
        {
            ConnectionState conn = kvp.Value;
            try
            {
                await NetworkHelpers.SendMessageAsync(conn.Stream, message);
            }
            catch
            {
                Console.WriteLine($"Error sending to {conn.Username}");
            }
        }
    }

    // 🧩 Gửi danh sách phòng đến toàn bộ user
    private async Task BroadcastRoomListAsync()
    {
        var roomList = new RoomListMessage
        {
            Type = "room_list",
            Rooms = new List<string>(_rooms.Keys)
        };

        foreach (var conn in _users.Values)
        {
            await NetworkHelpers.SendMessageAsync(conn.Stream, roomList);
        }
    }
}
