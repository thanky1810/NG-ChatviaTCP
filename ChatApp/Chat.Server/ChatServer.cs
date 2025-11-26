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

// (Người 1: Vũ Trí Dũng - Class lưu trạng thái kết nối)
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
    // (Người 1: Vũ Trí Dũng - Lõi Server)
    private readonly TcpListener _listener;

    // (Người 2: Thân Văn Ký - Quản lý User State)
    private readonly ConcurrentDictionary<string, ConnectionState> _users = new(StringComparer.OrdinalIgnoreCase);

    // (Người 3: Hoàng Mạnh Đức - Quản lý Room State)
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConnectionState>> _rooms = new(StringComparer.OrdinalIgnoreCase);
    // (Người 3: Hoàng Mạnh Đức - Lưu mật khẩu phòng)
    private readonly ConcurrentDictionary<string, string?> _roomPasswords = new(StringComparer.OrdinalIgnoreCase);

    // (Người 1: Vũ Trí Dũng - Events cho GUI Dashboard)
    public event Action<string>? LogMessageReceived;
    public event Action<List<string>>? UserListChanged;
    public event Action<List<string>>? RoomListChanged;

    public ChatServer(string ip, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ip), port);
        _rooms["public"] = new ConcurrentDictionary<string, ConnectionState>(StringComparer.OrdinalIgnoreCase);
        _roomPasswords["public"] = null;
    }

    // (Người 1: Vũ Trí Dũng - Vòng lặp chấp nhận kết nối)
    public async Task StartAsync()
    {
        try
        {
            _listener.Start();
            LogMessageReceived?.Invoke($"[INFO] Server listening on {_listener.LocalEndpoint}");
        }
        catch (Exception ex)
        {
            LogMessageReceived?.Invoke($"[CRITICAL] Cannot start server: {ex.Message}");
            return;
        }

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
                LogMessageReceived?.Invoke($"[ERROR] Accept: {ex.Message}");
            }
        }
    }

    // (Người 1: Vũ Trí Dũng - Vòng lặp xử lý từng client)
    private async Task HandleClientAsync(ConnectionState connection)
    {
        try
        {
            connection.Stream.ReadTimeout = 60000; // (Người 1: Timeout 60s cho Heartbeat)
            while (true)
            {
                BaseMessage message = await NetworkHelpers.ReadMessageAsync(connection.Stream);
                switch (message)
                {
                    // (Người 1: Xử lý Heartbeat)
                    case PingMessage:
                        await NetworkHelpers.SendMessageAsync(connection.Stream, new PongMessage());
                        break;

                    // (Người 2: Thân Văn Ký - Điều hướng Logic User)
                    case LoginMessage m: await HandleLoginAsync(connection, m); break;
                    case ChatPublicMessage m: await BroadcastPublicMessageAsync(connection, m); break;
                    case ChatPrivateMessage m: await HandlePrivateMessageAsync(connection, m); break;
                    case LogoutMessage:
                        LogMessageReceived?.Invoke($"[INFO] {connection.Username} requested logout.");
                        throw new IOException("Logout");

                    // (Người 3: Hoàng Mạnh Đức - Điều hướng Logic Room)
                    case CreateRoomMessage m: await HandleCreateRoomAsync(connection, m); break;
                    case JoinRoomMessage m: await HandleJoinRoomAsync(connection, m); break;
                    case LeaveRoomMessage m: await HandleLeaveRoomAsync(connection, m); break;
                    case ChatRoomMessage m: await BroadcastToRoomAsync(connection, m); break;

                    default:
                        LogMessageReceived?.Invoke($"[WARN] Unknown type from {connection.Username}");
                        break;
                }
            }
        }
        catch (IOException ex) { LogMessageReceived?.Invoke($"[INFO] Disconnected: {connection.Username ?? "?"} ({ex.Message})"); }
        catch (Exception ex) { LogMessageReceived?.Invoke($"[ERROR] {connection.Username}: {ex.Message}"); }
        finally
        {
            // (Người 2: Thân Văn Ký - Dọn dẹp khi ngắt kết nối)
            if (connection.Username != null)
            {
                _users.TryRemove(connection.Username, out _);
                // (Người 3: Dọn user khỏi phòng)
                foreach (var r in _rooms.Values) r.TryRemove(connection.Username, out _);

                LogMessageReceived?.Invoke($"[INFO] User {connection.Username} cleaned up.");
                await BroadcastUserListAsync();
                await BroadcastToAllAsync(new SystemMessage { Text = $"{connection.Username} đã rời khỏi chat." }, connection);
            }
            try { connection.Stream.Close(); connection.Client.Close(); } catch { }
        }
    }

    // (Người 2: Thân Văn Ký - Xử lý Đăng nhập)
    private async Task HandleLoginAsync(ConnectionState c, LoginMessage m)
    {
        if (_users.ContainsKey(m.Username))
        {
            await NetworkHelpers.SendMessageAsync(c.Stream, new ErrorMessage { Code = "username_taken", Message = "Tên đã tồn tại." });
            throw new IOException("Username taken");
        }
        c.Username = m.Username; _users[m.Username] = c;
        await NetworkHelpers.SendMessageAsync(c.Stream, new LoginOkMessage { Users = _users.Keys.ToList(), Rooms = _rooms.Keys.ToList() });
        await BroadcastUserListAsync();
        await BroadcastToAllAsync(new SystemMessage { Text = $"{m.Username} vừa tham gia." }, c);
    }

    // (Người 2: Thân Văn Ký - Xử lý Chat Public)
    private async Task BroadcastPublicMessageAsync(ConnectionState s, ChatPublicMessage m)
    {
        LogMessageReceived?.Invoke($"[PUBLIC] {s.Username}: {m.Text}");
        await BroadcastToAllAsync(new ChatPublicMessage { From = s.Username, Text = m.Text, Timestamp = DateTime.Now.ToString("o") });
    }

    // (Người 2: Thân Văn Ký - Xử lý Chat Private)
    private async Task HandlePrivateMessageAsync(ConnectionState s, ChatPrivateMessage m)
    {
        var msg = new ChatPrivateMessage { From = s.Username, To = m.To, Text = m.Text, Timestamp = DateTime.Now.ToString("o") };
        if (_users.TryGetValue(m.To, out var t)) await NetworkHelpers.SendMessageAsync(t.Stream, msg);
        else await NetworkHelpers.SendMessageAsync(s.Stream, new ErrorMessage { Message = "Người dùng không online." });

        if (s.Username != m.To) await NetworkHelpers.SendMessageAsync(s.Stream, msg);
    }

    // (Người 3: Hoàng Mạnh Đức - Xử lý Tạo phòng có mật khẩu)
    private async Task HandleCreateRoomAsync(ConnectionState connection, CreateRoomMessage msg)
    {
        if (_rooms.TryAdd(msg.Room, new ConcurrentDictionary<string, ConnectionState>(StringComparer.OrdinalIgnoreCase)))
        {
            _roomPasswords[msg.Room] = msg.Password; // Lưu mật khẩu
            _rooms[msg.Room][connection.Username] = connection;

            LogMessageReceived?.Invoke($"[ROOM] {connection.Username} created '{msg.Room}' (Pass: {!string.IsNullOrEmpty(msg.Password)})");
            await NetworkHelpers.SendMessageAsync(connection.Stream, new SystemMessage { Text = $"Đã tạo phòng '{msg.Room}'." });
            await BroadcastRoomListAsync();
        }
        else
        {
            await NetworkHelpers.SendMessageAsync(connection.Stream, new ErrorMessage { Code = "room_exists", Message = "Phòng đã tồn tại." });
        }
    }

    // (Người 3: Hoàng Mạnh Đức - Xử lý Vào phòng và check Pass)
    private async Task HandleJoinRoomAsync(ConnectionState connection, JoinRoomMessage msg)
    {
        if (!_rooms.TryGetValue(msg.Room, out var members))
        {
            await NetworkHelpers.SendMessageAsync(connection.Stream, new ErrorMessage { Code = "room_not_found", Message = "Không tìm thấy phòng." });
            return;
        }

        _roomPasswords.TryGetValue(msg.Room, out string? actualPass);
        if (!string.IsNullOrEmpty(actualPass) && actualPass != msg.Password)
        {
            await NetworkHelpers.SendMessageAsync(connection.Stream, new ErrorMessage { Code = "wrong_password", Message = "Sai mật khẩu phòng." });
            return;
        }

        members[connection.Username] = connection;
        LogMessageReceived?.Invoke($"[ROOM] {connection.Username} joined '{msg.Room}'");
        await NetworkHelpers.SendMessageAsync(connection.Stream, new SystemMessage { Text = $"Đã vào phòng '{msg.Room}'." });
        await BroadcastToRoomAsync(msg.Room, new SystemMessage { Text = $"{connection.Username} vào phòng." }, connection);
    }

    // (Người 3: Hoàng Mạnh Đức - Xử lý Rời phòng)
    private async Task HandleLeaveRoomAsync(ConnectionState c, LeaveRoomMessage m)
    {
        if (_rooms.TryGetValue(m.Room, out var mem) && mem.TryRemove(c.Username, out _))
        {
            LogMessageReceived?.Invoke($"[ROOM] {c.Username} left '{m.Room}'");
            await NetworkHelpers.SendMessageAsync(c.Stream, new SystemMessage { Text = $"Đã rời phòng '{m.Room}'." });
            await BroadcastToRoomAsync(m.Room, new SystemMessage { Text = $"{c.Username} rời phòng." }, null);
        }
    }

    // (Người 3: Hoàng Mạnh Đức - Xử lý Chat trong phòng)
    private async Task BroadcastToRoomAsync(ConnectionState s, ChatRoomMessage m)
    {
        if (_rooms.TryGetValue(m.Room, out var mem) && mem.ContainsKey(s.Username))
        {
            LogMessageReceived?.Invoke($"[ROOM] {s.Username}@{m.Room}: {m.Text}");
            var msg = new ChatRoomMessage { From = s.Username, Room = m.Room, Text = m.Text, Timestamp = DateTime.Now.ToString("o") };
            foreach (var u in mem.Values) await NetworkHelpers.SendMessageAsync(u.Stream, msg);
        }
        else await NetworkHelpers.SendMessageAsync(s.Stream, new ErrorMessage { Message = "Bạn không ở trong phòng này." });
    }

    // (Người 1 & 2 & 3: Các hàm tiện ích Broadcast)
    private async Task BroadcastToAllAsync(BaseMessage m, ConnectionState? ex = null)
    {
        foreach (var u in _users.Values) if (u != ex) await NetworkHelpers.SendMessageAsync(u.Stream, m);
    }

    private async Task BroadcastToRoomAsync(string r, BaseMessage m, ConnectionState? ex = null)
    {
        if (_rooms.TryGetValue(r, out var mem)) foreach (var u in mem.Values) if (u != ex) await NetworkHelpers.SendMessageAsync(u.Stream, m);
    }

    private async Task BroadcastUserListAsync()
    {
        var u = _users.Keys.ToList(); UserListChanged?.Invoke(u); await BroadcastToAllAsync(new UserListMessage { Users = u });
    }

    private async Task BroadcastRoomListAsync()
    {
        var r = _rooms.Keys.ToList(); RoomListChanged?.Invoke(r); await BroadcastToAllAsync(new RoomListMessage { Rooms = r });
    }
}