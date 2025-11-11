// File: Chat.Server/ChatServer.cs
// (Người 1: Lõi Server, Quản lý Kết nối)
// (Người 2: Logic User & Vòng đời)
// (Người 3: Logic Phòng)
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

// (Người 1) Lớp lưu trữ trạng thái của mỗi kết nối client
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

    // (Người 2 & 3) Cấu trúc quản lý State (thread-safe)
    private readonly ConcurrentDictionary<string, ConnectionState> _users = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConnectionState>> _rooms = new(StringComparer.OrdinalIgnoreCase);

    // (Người 1) Sự kiện (Events) để gửi thông tin lên Giao diện (ServerForm)
    public event Action<string>? LogMessageReceived;
    public event Action<List<string>>? UserListChanged;
    public event Action<List<string>>? RoomListChanged;

    public ChatServer(string ip, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ip), port);
        // (Người 3) Khởi tạo phòng 'public' mặc định
        _rooms["public"] = new ConcurrentDictionary<string, ConnectionState>(StringComparer.OrdinalIgnoreCase);
    }

    // (Người 1) Lõi Server: Vòng lặp lắng nghe và chấp nhận client
    public async Task StartAsync()
    {
        _listener.Start();
        LogMessageReceived?.Invoke($"[INFO] Server listening on {_listener.LocalEndpoint}");

        // Cập nhật GUI lúc khởi động
        RoomListChanged?.Invoke(new List<string>(_rooms.Keys.OrderBy(r => r)));
        UserListChanged?.Invoke(new List<string>(_users.Keys.OrderBy(u => u)));

        while (true)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                LogMessageReceived?.Invoke($"[INFO] New client connected: {client.Client.RemoteEndPoint}");
                var connection = new ConnectionState(client);

                // (Người 1) Tạo một Task riêng để xử lý client này, không chặn vòng lặp
                _ = HandleClientAsync(connection);
            }
            catch (Exception ex)
            {
                LogMessageReceived?.Invoke($"[ERROR] Error accepting client: {ex.Message}");
            }
        }
    }

    // (Người 1) Vòng lặp đọc message cho mỗi client
    private async Task HandleClientAsync(ConnectionState connection)
    {
        try
        {
            while (true)
            {
                // (Người 1) Đọc message đã được đóng gói (Framing)
                BaseMessage message = await NetworkHelpers.ReadMessageAsync(connection.Stream);

                // Phân loại message và chuyển đến bộ xử lý logic tương ứng
                switch (message)
                {
                    // (Người 2) UC-01: Đăng nhập
                    case LoginMessage login:
                        await HandleLoginAsync(connection, login);
                        break;

                    // (Người 2) UC-02: Chat công khai
                    case ChatPublicMessage chatPublic:
                        await BroadcastPublicMessageAsync(connection, chatPublic);
                        break;

                    // (Người 2) UC-03: Chat riêng
                    case ChatPrivateMessage dm:
                        await HandlePrivateMessageAsync(connection, dm);
                        break;

                    // (Người 3) UC-04: Tạo phòng
                    case CreateRoomMessage createRoom:
                        await HandleCreateRoomAsync(connection, createRoom);
                        break;

                    // (Người 3) UC-04: Vào phòng
                    case JoinRoomMessage joinRoom:
                        await HandleJoinRoomAsync(connection, joinRoom);
                        break;

                    // (Người 3) UC-04: Rời phòng
                    case LeaveRoomMessage leaveRoom:
                        await HandleLeaveRoomAsync(connection, leaveRoom);
                        break;

                    // (Người 3) Chat trong phòng
                    case ChatRoomMessage chatRoom:
                        await BroadcastToRoomAsync(connection, chatRoom);
                        break;

                    // (Người 2) UC-06: Logout
                    case LogoutMessage:
                        LogMessageReceived?.Invoke($"[INFO] {connection.Username} requested logout.");
                        throw new IOException("User requested logout."); // Thoát vòng lặp, đi đến 'finally'

                    default:
                        LogMessageReceived?.Invoke($"[WARN] Unknown message type from {connection.Username}");
                        break;
                }
            }
        }
        catch (IOException ex) // Xử lý ngắt kết nối
        {
            LogMessageReceived?.Invoke($"[INFO] Client disconnected: {connection.Username ?? "pending login"} | Reason: {ex.Message}");
        }
        catch (Exception ex) // Các lỗi logic/parse khác
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
            // (Người 2) Dọn dẹp User sau khi thoát (UC-06 hoặc ngắt kết nối)
            if (connection.Username != null)
            {
                // 1. Xóa khỏi danh sách user
                _users.TryRemove(connection.Username, out _);

                // 2. (Người 2 & 3) Xóa user khỏi tất cả các phòng
                foreach (var roomName in _rooms.Keys)
                {
                    if (_rooms.TryGetValue(roomName, out var members))
                    {
                        members.TryRemove(connection.Username, out _);
                    }
                }
                LogMessageReceived?.Invoke($"[INFO] User {connection.Username} cleaned up.");

                // 3. (Người 2) UC-05: Gửi danh sách user mới
                await BroadcastUserListAsync();

                // 4. (Người 2) Gửi thông báo hệ thống
                var sysMsg = new SystemMessage { Text = $"{connection.Username} đã rời khỏi chat." };
                await BroadcastToAllAsync(sysMsg, connection); // Gửi cho mọi người (trừ người vừa thoát)
            }

            // (Người 1) Đóng kết nối
            try
            {
                connection.Stream.Close();
                connection.Client.Close();
            }
            catch { }
        }
    }

    // --- (Người 2) Các hàm xử lý nghiệp vụ User ---

    // (Người 2) UC-01: Đăng nhập
    private async Task HandleLoginAsync(ConnectionState connection, LoginMessage login)
    {
        // 1. Kiểm tra trùng tên
        if (_users.ContainsKey(login.Username))
        {
            LogMessageReceived?.Invoke($"[WARN] Login failed: {login.Username} (username_taken)");
            var err = new ErrorMessage { Code = "username_taken", Message = $"Tên '{login.Username}' đã được sử dụng." };
            await NetworkHelpers.SendMessageAsync(connection.Stream, err);
            throw new IOException("Login failed: username taken.");
        }

        // 2. Lưu trạng thái
        connection.Username = login.Username;
        _users[login.Username] = connection;
        LogMessageReceived?.Invoke($"[INFO] User logged in: {login.Username}");

        // 3. Trả về LoginOk (gồm danh sách users và rooms ban đầu)
        var ok = new LoginOkMessage
        {
            Users = new List<string>(_users.Keys.OrderBy(u => u)),
            Rooms = new List<string>(_rooms.Keys.OrderBy(r => r))
        };
        await NetworkHelpers.SendMessageAsync(connection.Stream, ok);

        // 4. (Người 2) UC-05: Cập nhật user list cho người khác
        await BroadcastUserListAsync();

        // 5. Gửi thông báo hệ thống
        var sysMsg = new SystemMessage { Text = $"{login.Username} vừa tham gia chat." };
        await BroadcastToAllAsync(sysMsg, connection);
    }

    // (Người 2) UC-02: Chat công khai (Broadcast)
    private async Task BroadcastPublicMessageAsync(ConnectionState sender, ChatPublicMessage chat)
    {
        LogMessageReceived?.Invoke($"[PUBLIC] {sender.Username}: {chat.Text}");
        var msg = new ChatPublicMessage
        {
            From = sender.Username ?? "unknown",
            Text = chat.Text,
            Timestamp = DateTime.UtcNow.ToString("o") // Gán thời gian chuẩn ISO 8601
        };
        await BroadcastToAllAsync(msg, null); // Gửi cho tất cả (kể cả người gửi)
    }

    // (Người 2) UC-03: Chat riêng (DM)
    private async Task HandlePrivateMessageAsync(ConnectionState sender, ChatPrivateMessage dm)
    {
        LogMessageReceived?.Invoke($"[DM] {sender.Username} -> {dm.To}: {dm.Text}");
        var msg = new ChatPrivateMessage
        {
            From = sender.Username ?? "unknown",
            To = dm.To,
            Text = dm.Text,
            Timestamp = DateTime.UtcNow.ToString("o")
        };

        // 1. Gửi cho người nhận
        if (_users.TryGetValue(dm.To, out var targetConn))
        {
            await NetworkHelpers.SendMessageAsync(targetConn.Stream, msg);
        }
        else
        {
            // (Ngoại lệ) Người nhận offline
            var err = new ErrorMessage { Code = "user_offline", Message = $"Người dùng '{dm.To}' không online." };
            await NetworkHelpers.SendMessageAsync(sender.Stream, err);
        }

        // 2. Gửi lại bản sao cho người gửi (để UI người gửi hiển thị)
        if (string.Compare(sender.Username, dm.To, StringComparison.OrdinalIgnoreCase) != 0)
        {
            await NetworkHelpers.SendMessageAsync(sender.Stream, msg);
        }
    }

    // --- (Người 3) Các hàm xử lý nghiệp vụ Phòng ---

    // (Người 3) UC-04: Tạo phòng
    private async Task HandleCreateRoomAsync(ConnectionState connection, CreateRoomMessage createRoom)
    {
        string room = createRoom.Room;
        string creator = connection.Username ?? "unknown";

        if (_rooms.TryAdd(room, new ConcurrentDictionary<string, ConnectionState>(StringComparer.OrdinalIgnoreCase)))
        {
            LogMessageReceived?.Invoke($"[ROOM] {creator} created room '{room}'");

            // Tự động cho người tạo vào phòng
            _rooms[room][creator] = connection;

            var ok = new SystemMessage { Text = $"Đã tạo phòng '{room}'." };
            await NetworkHelpers.SendMessageAsync(connection.Stream, ok);

            // Cập nhật danh sách phòng cho mọi người
            await BroadcastRoomListAsync();
        }
        else
        {
            // Phòng đã tồn tại
            var err = new ErrorMessage { Code = "room_exists", Message = $"Phòng '{room}' đã tồn tại." };
            await NetworkHelpers.SendMessageAsync(connection.Stream, err);
        }
    }

    // (Người 3) UC-04: Vào phòng
    private async Task HandleJoinRoomAsync(ConnectionState connection, JoinRoomMessage joinRoom)
    {
        string room = joinRoom.Room;
        string user = connection.Username ?? "unknown";

        if (!_rooms.TryGetValue(room, out var members))
        {
            var err = new ErrorMessage { Code = "room_not_found", Message = $"Không tìm thấy phòng '{room}'." };
            await NetworkHelpers.SendMessageAsync(connection.Stream, err);
            return;
        }

        members[user] = connection;
        LogMessageReceived?.Invoke($"[ROOM] {user} joined room '{room}'");

        var ok = new SystemMessage { Text = $"Bạn đã tham gia phòng '{room}'." };
        await NetworkHelpers.SendMessageAsync(connection.Stream, ok);

        var sysMsg = new SystemMessage { Text = $"{user} vừa tham gia phòng." };
        await BroadcastToRoomAsync(room, sysMsg, connection);
    }

    // (Người 3) UC-04: Rời phòng
    private async Task HandleLeaveRoomAsync(ConnectionState connection, LeaveRoomMessage leaveRoom)
    {
        string room = leaveRoom.Room;
        string user = connection.Username ?? "unknown";

        if (_rooms.TryGetValue(room, out var members))
        {
            if (members.TryRemove(user, out _))
            {
                LogMessageReceived?.Invoke($"[ROOM] {user} left room '{room}'");
                var ok = new SystemMessage { Text = $"Bạn đã rời phòng '{room}'." };
                await NetworkHelpers.SendMessageAsync(connection.Stream, ok);

                var sysMsg = new SystemMessage { Text = $"{user} đã rời phòng." };
                await BroadcastToRoomAsync(room, sysMsg, null);
            }
        }
    }

    // (Người 3) Chat trong phòng
    private async Task BroadcastToRoomAsync(ConnectionState sender, ChatRoomMessage chatRoom)
    {
        string room = chatRoom.Room;
        string senderName = sender.Username ?? "unknown";

        // Kiểm tra xem người gửi có phải là thành viên phòng không
        if (!_rooms.TryGetValue(room, out var members) || !members.ContainsKey(senderName))
        {
            var err = new ErrorMessage { Code = "not_in_room", Message = "Bạn không ở trong phòng này." };
            await NetworkHelpers.SendMessageAsync(sender.Stream, err);
            return;
        }

        LogMessageReceived?.Invoke($"[ROOM] {senderName}@{room}: {chatRoom.Text}");
        var msg = new ChatRoomMessage
        {
            Room = room,
            From = senderName,
            Text = chatRoom.Text,
            Timestamp = DateTime.UtcNow.ToString("o")
        };
        await BroadcastToRoomAsync(room, msg, null); // Gửi cho mọi người (kể cả người gửi)
    }

    // --- Các hàm Broadcast (gửi cho nhiều người) ---

    // (Người 1) Gửi 1 message cho TẤT CẢ user đang online
    private async Task BroadcastToAllAsync(BaseMessage message, ConnectionState? excludeConnection = null)
    {
        var tasks = new List<Task>();
        foreach (var conn in _users.Values)
        {
            if (conn != excludeConnection)
            {
                tasks.Add(NetworkHelpers.SendMessageAsync(conn.Stream, message));
            }
        }
        await Task.WhenAll(tasks);
    }

    // (Người 3) Gửi 1 message cho TẤT CẢ user trong 1 phòng
    private async Task BroadcastToRoomAsync(string room, BaseMessage message, ConnectionState? excludeConnection = null)
    {
        if (!_rooms.TryGetValue(room, out var members)) return;
        var tasks = new List<Task>();
        foreach (var conn in members.Values)
        {
            if (conn != excludeConnection)
            {
                tasks.Add(NetworkHelpers.SendMessageAsync(conn.Stream, message));
            }
        }
        await Task.WhenAll(tasks);
    }

    // (Người 2) UC-05: Gửi danh sách user mới
    private async Task BroadcastUserListAsync()
    {
        var users = new List<string>(_users.Keys.OrderBy(u => u));

        // 1. Cập nhật Giao diện (ServerForm)
        UserListChanged?.Invoke(users);

        // 2. Cập nhật tất cả Client
        var userListMsg = new UserListMessage { Users = users };
        await BroadcastToAllAsync(userListMsg, null);
    }

    // (Người 3) Gửi danh sách phòng mới
    private async Task BroadcastRoomListAsync()
    {
        var rooms = new List<string>(_rooms.Keys.OrderBy(r => r));

        // 1. Cập nhật Giao diện (ServerForm)
        RoomListChanged?.Invoke(rooms);

        // 2. Cập nhật tất cả Client
        var roomListMsg = new RoomListMessage { Rooms = rooms };
        await BroadcastToAllAsync(roomListMsg, null);
    }
}