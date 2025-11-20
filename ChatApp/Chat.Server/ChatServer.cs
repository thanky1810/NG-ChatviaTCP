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

    // ✅ THÊM: Biến lưu mật khẩu phòng
    private readonly ConcurrentDictionary<string, string?> _roomPasswords = new(StringComparer.OrdinalIgnoreCase);

    public event Action<string>? LogMessageReceived;
    public event Action<List<string>>? UserListChanged;
    public event Action<List<string>>? RoomListChanged;

    public ChatServer(string ip, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ip), port);
        _rooms["public"] = new ConcurrentDictionary<string, ConnectionState>(StringComparer.OrdinalIgnoreCase);
        _roomPasswords["public"] = null; // Phòng public không pass
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
            catch (Exception ex) { LogMessageReceived?.Invoke($"[ERROR] Accept: {ex.Message}"); }
        }
    }

    private async Task HandleClientAsync(ConnectionState connection)
    {
        try
        {
            connection.Stream.ReadTimeout = 60000;
            while (true)
            {
                BaseMessage message = await NetworkHelpers.ReadMessageAsync(connection.Stream);
                switch (message)
                {
                    case PingMessage: await NetworkHelpers.SendMessageAsync(connection.Stream, new PongMessage()); break;
                    case LoginMessage m: await HandleLoginAsync(connection, m); break;
                    case ChatPublicMessage m: await BroadcastPublicMessageAsync(connection, m); break;
                    case ChatPrivateMessage m: await HandlePrivateMessageAsync(connection, m); break;

                    // ✅ CẬP NHẬT CÁC HÀM XỬ LÝ PHÒNG
                    case CreateRoomMessage m: await HandleCreateRoomAsync(connection, m); break;
                    case JoinRoomMessage m: await HandleJoinRoomAsync(connection, m); break;

                    case LeaveRoomMessage m: await HandleLeaveRoomAsync(connection, m); break;
                    case ChatRoomMessage m: await BroadcastToRoomAsync(connection, m); break;
                    case LogoutMessage:
                        LogMessageReceived?.Invoke($"[INFO] {connection.Username} logout.");
                        throw new IOException("Logout");
                    default:
                        LogMessageReceived?.Invoke($"[WARN] Unknown type from {connection.Username}");
                        break;
                }
            }
        }
        catch (IOException ex) { LogMessageReceived?.Invoke($"[INFO] Disconnected: {connection.Username} ({ex.Message})"); }
        catch (Exception ex) { LogMessageReceived?.Invoke($"[ERROR] {connection.Username}: {ex.Message}"); }
        finally
        {
            if (connection.Username != null)
            {
                _users.TryRemove(connection.Username, out _);
                foreach (var r in _rooms.Values) r.TryRemove(connection.Username, out _);
                LogMessageReceived?.Invoke($"[INFO] User {connection.Username} cleaned up.");
                await BroadcastUserListAsync();
                await BroadcastToAllAsync(new SystemMessage { Text = $"{connection.Username} đã rời khỏi chat." }, connection);
            }
            try { connection.Stream.Close(); connection.Client.Close(); } catch { }
        }
    }

    // --- (Các hàm Login, ChatPublic, ChatPrivate giữ nguyên, tôi rút gọn) ---
    private async Task HandleLoginAsync(ConnectionState c, LoginMessage m)
    {
        if (_users.ContainsKey(m.Username))
        {
            await NetworkHelpers.SendMessageAsync(c.Stream, new ErrorMessage { Code = "username_taken", Message = "Trùng tên" });
            throw new IOException("Username taken");
        }
        c.Username = m.Username; _users[m.Username] = c;
        await NetworkHelpers.SendMessageAsync(c.Stream, new LoginOkMessage { Users = _users.Keys.ToList(), Rooms = _rooms.Keys.ToList() });
        await BroadcastUserListAsync();
        await BroadcastToAllAsync(new SystemMessage { Text = $"{m.Username} joined." }, c);
    }
    private async Task BroadcastPublicMessageAsync(ConnectionState s, ChatPublicMessage m)
    {
        await BroadcastToAllAsync(new ChatPublicMessage { From = s.Username, Text = m.Text, Timestamp = DateTime.Now.ToString("o") });
    }
    private async Task HandlePrivateMessageAsync(ConnectionState s, ChatPrivateMessage m)
    {
        var msg = new ChatPrivateMessage { From = s.Username, To = m.To, Text = m.Text, Timestamp = DateTime.Now.ToString("o") };
        if (_users.TryGetValue(m.To, out var t)) await NetworkHelpers.SendMessageAsync(t.Stream, msg);
        else await NetworkHelpers.SendMessageAsync(s.Stream, new ErrorMessage { Message = "User offline" });
        if (s.Username != m.To) await NetworkHelpers.SendMessageAsync(s.Stream, msg);
    }

    // ✅ XỬ LÝ TẠO PHÒNG (Lưu mật khẩu)
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

    // ✅ XỬ LÝ VÀO PHÒNG (Kiểm tra mật khẩu)
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

    private async Task HandleLeaveRoomAsync(ConnectionState c, LeaveRoomMessage m)
    {
        if (_rooms.TryGetValue(m.Room, out var mem) && mem.TryRemove(c.Username, out _))
        {
            LogMessageReceived?.Invoke($"[ROOM] {c.Username} left '{m.Room}'");
            await NetworkHelpers.SendMessageAsync(c.Stream, new SystemMessage { Text = $"Đã rời phòng '{m.Room}'." });
            await BroadcastToRoomAsync(m.Room, new SystemMessage { Text = $"{c.Username} rời phòng." }, null);
        }
    }
    private async Task BroadcastToRoomAsync(ConnectionState s, ChatRoomMessage m)
    {
        if (_rooms.TryGetValue(m.Room, out var mem) && mem.ContainsKey(s.Username))
        {
            var msg = new ChatRoomMessage { From = s.Username, Room = m.Room, Text = m.Text, Timestamp = DateTime.Now.ToString("o") };
            foreach (var u in mem.Values) await NetworkHelpers.SendMessageAsync(u.Stream, msg);
        }
        else await NetworkHelpers.SendMessageAsync(s.Stream, new ErrorMessage { Message = "Not in room" });
    }
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