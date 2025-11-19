// File: Chat.Server/ChatServer.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private readonly ConcurrentDictionary<string, ConnectionState> _users = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConnectionState>> _rooms = new(StringComparer.OrdinalIgnoreCase);

    public event Action<string>? LogMessageReceived;
    public event Action<List<string>>? UserListChanged;
    public event Action<List<string>>? RoomListChanged;

    public ChatServer(string ip, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ip), port);
        _rooms["public"] = new ConcurrentDictionary<string, ConnectionState>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        LogMessageReceived?.Invoke($"[INFO] Server listening on {_listener.LocalEndpoint}");

        RoomListChanged?.Invoke(new List<string>(_rooms.Keys.OrderBy(r => r)));
        UserListChanged?.Invoke(new List<string>(_users.Keys.OrderBy(u => u)));

        while (true)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                LogMessageReceived?.Invoke($"[INFO] New client connected: {client.Client.RemoteEndPoint}");
                var connection = new ConnectionState(client);
                _ = HandleClientAsync(connection);
            }
            catch (Exception ex)
            {
                LogMessageReceived?.Invoke($"[ERROR] Error accepting client: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(ConnectionState connection)
    {
        try
        {
            // ✅ HEARTBEAT: Timeout 60s. Nếu không nhận được gì (kể cả Ping) thì ngắt.
            connection.Stream.ReadTimeout = 60000;

            while (true)
            {
                BaseMessage message = await NetworkHelpers.ReadMessageAsync(connection.Stream);
                switch (message)
                {
                    // ✅ HEARTBEAT: Nhận Ping -> Trả lời Pong
                    case PingMessage:
                        // (Không cần log để đỡ rác, hoặc log debug nếu muốn)
                        await NetworkHelpers.SendMessageAsync(connection.Stream, new PongMessage());
                        break;

                    case LoginMessage login: await HandleLoginAsync(connection, login); break;
                    case ChatPublicMessage chatPublic: await BroadcastPublicMessageAsync(connection, chatPublic); break;
                    case ChatPrivateMessage dm: await HandlePrivateMessageAsync(connection, dm); break;
                    case CreateRoomMessage createRoom: await HandleCreateRoomAsync(connection, createRoom); break;
                    case JoinRoomMessage joinRoom: await HandleJoinRoomAsync(connection, joinRoom); break;
                    case LeaveRoomMessage leaveRoom: await HandleLeaveRoomAsync(connection, leaveRoom); break;
                    case ChatRoomMessage chatRoom: await BroadcastToRoomAsync(connection, chatRoom); break;

                    case LogoutMessage:
                        LogMessageReceived?.Invoke($"[INFO] {connection.Username} requested logout.");
                        throw new IOException("User requested logout.");

                    default:
                        LogMessageReceived?.Invoke($"[WARN] Unknown message type from {connection.Username}");
                        break;
                }
            }
        }
        catch (IOException ex)
        {
            LogMessageReceived?.Invoke($"[INFO] Client disconnected: {connection.Username ?? "pending login"} | Reason: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogMessageReceived?.Invoke($"[ERROR] Error handling client {connection.Username}: {ex.Message}");
            try
            {
                var err = new ErrorMessage { Code = "server_error", Message = ex.Message };
                await NetworkHelpers.SendMessageAsync(connection.Stream, err);
            }
            catch { }
        }
        finally
        {
            if (connection.Username != null)
            {
                _users.TryRemove(connection.Username, out _);
                foreach (var roomName in _rooms.Keys)
                {
                    if (_rooms.TryGetValue(roomName, out var members))
                    {
                        members.TryRemove(connection.Username, out _);
                    }
                }
                LogMessageReceived?.Invoke($"[INFO] User {connection.Username} cleaned up.");
                await BroadcastUserListAsync();
                var sysMsg = new SystemMessage { Text = $"{connection.Username} đã rời khỏi chat." };
                await BroadcastToAllAsync(sysMsg, connection);
            }
            try { connection.Stream.Close(); connection.Client.Close(); } catch { }
        }
    }

    // --- Các hàm logic nghiệp vụ (Giữ nguyên) ---
    private async Task HandleLoginAsync(ConnectionState connection, LoginMessage login)
    {
        if (_users.ContainsKey(login.Username))
        {
            LogMessageReceived?.Invoke($"[WARN] Login failed: {login.Username} (username_taken)");
            var err = new ErrorMessage { Code = "username_taken", Message = $"Tên '{login.Username}' đã được sử dụng." };
            await NetworkHelpers.SendMessageAsync(connection.Stream, err);
            throw new IOException("Login failed: username taken.");
        }
        connection.Username = login.Username;
        _users[login.Username] = connection;
        LogMessageReceived?.Invoke($"[INFO] User logged in: {login.Username}");
        var ok = new LoginOkMessage
        {
            Users = new List<string>(_users.Keys.OrderBy(u => u)),
            Rooms = new List<string>(_rooms.Keys.OrderBy(r => r))
        };
        await NetworkHelpers.SendMessageAsync(connection.Stream, ok);
        await BroadcastUserListAsync();
        var sysMsg = new SystemMessage { Text = $"{login.Username} vừa tham gia chat." };
        await BroadcastToAllAsync(sysMsg, connection);
    }

    private async Task BroadcastPublicMessageAsync(ConnectionState sender, ChatPublicMessage chat)
    {
        LogMessageReceived?.Invoke($"[PUBLIC] {sender.Username}: {chat.Text}");
        var msg = new ChatPublicMessage { From = sender.Username ?? "unknown", Text = chat.Text, Timestamp = DateTime.UtcNow.ToString("o") };
        await BroadcastToAllAsync(msg, null);
    }

    private async Task HandlePrivateMessageAsync(ConnectionState sender, ChatPrivateMessage dm)
    {
        LogMessageReceived?.Invoke($"[DM] {sender.Username} -> {dm.To}: {dm.Text}");
        var msg = new ChatPrivateMessage { From = sender.Username ?? "unknown", To = dm.To, Text = dm.Text, Timestamp = DateTime.UtcNow.ToString("o") };
        if (_users.TryGetValue(dm.To, out var targetConn)) await NetworkHelpers.SendMessageAsync(targetConn.Stream, msg);
        else await NetworkHelpers.SendMessageAsync(sender.Stream, new ErrorMessage { Code = "user_offline", Message = $"Người dùng '{dm.To}' không online." });
        if (string.Compare(sender.Username, dm.To, StringComparison.OrdinalIgnoreCase) != 0) await NetworkHelpers.SendMessageAsync(sender.Stream, msg);
    }

    private async Task HandleCreateRoomAsync(ConnectionState connection, CreateRoomMessage createRoom)
    {
        string room = createRoom.Room;
        string creator = connection.Username ?? "unknown";
        if (_rooms.TryAdd(room, new ConcurrentDictionary<string, ConnectionState>(StringComparer.OrdinalIgnoreCase)))
        {
            LogMessageReceived?.Invoke($"[ROOM] {creator} created room '{room}'");
            _rooms[room][creator] = connection;
            await NetworkHelpers.SendMessageAsync(connection.Stream, new SystemMessage { Text = $"Đã tạo phòng '{room}'." });
            await BroadcastRoomListAsync();
        }
        else
        {
            await NetworkHelpers.SendMessageAsync(connection.Stream, new ErrorMessage { Code = "room_exists", Message = $"Phòng '{room}' đã tồn tại." });
        }
    }

    private async Task HandleJoinRoomAsync(ConnectionState connection, JoinRoomMessage joinRoom)
    {
        string room = joinRoom.Room;
        string user = connection.Username ?? "unknown";
        if (!_rooms.TryGetValue(room, out var members))
        {
            await NetworkHelpers.SendMessageAsync(connection.Stream, new ErrorMessage { Code = "room_not_found", Message = $"Không tìm thấy phòng '{room}'." });
            return;
        }
        members[user] = connection;
        LogMessageReceived?.Invoke($"[ROOM] {user} joined room '{room}'");
        await NetworkHelpers.SendMessageAsync(connection.Stream, new SystemMessage { Text = $"Bạn đã tham gia phòng '{room}'." });
        await BroadcastToRoomAsync(room, new SystemMessage { Text = $"{user} vừa tham gia phòng." }, connection);
    }

    private async Task HandleLeaveRoomAsync(ConnectionState connection, LeaveRoomMessage leaveRoom)
    {
        string room = leaveRoom.Room;
        string user = connection.Username ?? "unknown";
        if (_rooms.TryGetValue(room, out var members))
        {
            if (members.TryRemove(user, out _))
            {
                LogMessageReceived?.Invoke($"[ROOM] {user} left room '{room}'");
                await NetworkHelpers.SendMessageAsync(connection.Stream, new SystemMessage { Text = $"Bạn đã rời phòng '{room}'." });
                await BroadcastToRoomAsync(room, new SystemMessage { Text = $"{user} đã rời phòng." }, null);
            }
        }
    }

    private async Task BroadcastToRoomAsync(ConnectionState sender, ChatRoomMessage chatRoom)
    {
        string room = chatRoom.Room;
        string senderName = sender.Username ?? "unknown";
        if (!_rooms.TryGetValue(room, out var members) || !members.ContainsKey(senderName))
        {
            await NetworkHelpers.SendMessageAsync(sender.Stream, new ErrorMessage { Code = "not_in_room", Message = "Bạn không ở trong phòng này." });
            return;
        }
        LogMessageReceived?.Invoke($"[ROOM] {senderName}@{room}: {chatRoom.Text}");
        var msg = new ChatRoomMessage { Room = room, From = senderName, Text = chatRoom.Text, Timestamp = DateTime.UtcNow.ToString("o") };
        await BroadcastToRoomAsync(room, msg, null);
    }

    private async Task BroadcastToAllAsync(BaseMessage message, ConnectionState? excludeConnection = null)
    {
        var tasks = new List<Task>();
        foreach (var conn in _users.Values) { if (conn != excludeConnection) tasks.Add(NetworkHelpers.SendMessageAsync(conn.Stream, message)); }
        await Task.WhenAll(tasks);
    }

    private async Task BroadcastToRoomAsync(string room, BaseMessage message, ConnectionState? excludeConnection = null)
    {
        if (!_rooms.TryGetValue(room, out var members)) return;
        var tasks = new List<Task>();
        foreach (var conn in members.Values) { if (conn != excludeConnection) tasks.Add(NetworkHelpers.SendMessageAsync(conn.Stream, message)); }
        await Task.WhenAll(tasks);
    }

    private async Task BroadcastUserListAsync()
    {
        var users = new List<string>(_users.Keys.OrderBy(u => u));
        UserListChanged?.Invoke(users);
        await BroadcastToAllAsync(new UserListMessage { Users = users }, null);
    }

    private async Task BroadcastRoomListAsync()
    {
        var rooms = new List<string>(_rooms.Keys.OrderBy(r => r));
        RoomListChanged?.Invoke(rooms);
        await BroadcastToAllAsync(new RoomListMessage { Rooms = rooms }, null);
    }
}