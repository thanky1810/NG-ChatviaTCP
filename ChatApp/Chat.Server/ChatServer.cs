// File: Chat.Server/ChatServer.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Chat.Shared; // <-- QUAN TRỌNG: Thêm reference

namespace Chat.Server;

// (Gợi ý mục 10.1) Lớp quản lý kết nối
public class ConnectionState
{
    public TcpClient Client { get; }
    public NetworkStream Stream { get; }
    public string? Username { get; set; } // Sẽ được set sau khi login

    public ConnectionState(TcpClient client)
    {
        Client = client;
        Stream = client.GetStream();
    }
}

public class ChatServer
{
    private readonly TcpListener _listener;

    // TODO (Người 2 & 3): Sử dụng các cấu trúc dữ liệu này
    // (Mục 10.1: Quản lý state của server)
    // Đảm bảo dùng ConcurrentDictionary vì sẽ có nhiều luồng truy cập
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

        // Vòng lặp vô tận để chấp nhận client mới (mục 6.1)
        while (true)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine($"New client connected: {client.Client.RemoteEndPoint}");

                // Tạo một ConnectionState mới
                var connection = new ConnectionState(client);

                // Chạy HandleClientAsync trong một Task mới (không await)
                // để vòng lặp accept có thể tiếp tục
                _ = HandleClientAsync(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting client: {ex.Message}");
            }
        }
    }

    // Hàm này xử lý vòng đời của 1 client
    private async Task HandleClientAsync(ConnectionState connection)
    {
        try
        {
            // Vòng lặp đọc tin nhắn từ client này
            while (true)
            {
                // Đọc message (đã xử lý length-prefix)
                BaseMessage message = await NetworkHelpers.ReadMessageAsync(connection.Stream);

                // TODO (Người 2 & 3): Bắt đầu xử lý logic tại đây
                // Dùng switch-case với (message.Type) hoặc (message)
                switch (message)
                {
                    case LoginMessage login:
                        // TODO (Người 2): Xử lý UC-01
                        // - Kiểm tra trùng tên trong _users
                        // - Nếu OK: connection.Username = login.Username;
                        //           _users.TryAdd(login.Username, connection);
                        //           Gửi LoginOkMessage
                        //           Broadcast UserList mới
                        // - Nếu Fail: Gửi ErrorMessage ("username_taken")
                        Console.WriteLine($"Received Login from {login.Username}");
                        break;

                    case ChatPublicMessage chat:
                        // TODO (Người 2): Xử lý UC-02
                        // - Broadcast (gửi lại) message này cho TẤT CẢ user trong _users
                        Console.WriteLine($"Received Public Chat from {chat.From}: {chat.Text}");
                        break;

                    case ChatPrivateMessage dm:
                        // TODO (Người 2): Xử lý UC-03
                        // - Tìm user 'dm.To' trong _users
                        // - Nếu thấy -> Gửi cho họ
                        // - Nếu không thấy -> Gửi ErrorMessage ("user_offline")
                        break;

                    case CreateRoomMessage createRoom:
                        // TODO (Người 3): Xử lý UC-04 (Create)
                        break;

                    case JoinRoomMessage joinRoom:
                        // TODO (Người 3): Xử lý UC-04 (Join)
                        break;

                    case LogoutMessage logout:
                        // TODO (Người 2): Xử lý UC-06
                        // - Đóng socket và gọi hàm Cleanup
                        throw new IOException("User requested logout.");

                    // Thêm các case khác...

                    default:
                        Console.WriteLine($"Unknown message type: {message.Type}");
                        break;
                }
            }
        }
        catch (IOException ex)
        {
            // Client ngắt kết nối (chủ động logout hoặc rớt mạng)
            Console.WriteLine($"Client disconnected: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Lỗi deserialize hoặc lỗi logic
            Console.WriteLine($"Error handling client: {ex.Message}");
            // (Tùy chọn) Gửi ErrorMessage về client nếu có thể
        }
        finally
        {
            // --- TODO (Người 2 & 3): Xử lý Cleanup ---
            // Đây là phần quan trọng khi client logout (UC-06) hoặc rớt mạng (AC-05)

            // 1. (Người 2) Xóa user khỏi danh sách online
            if (connection.Username != null)
            {
                _users.TryRemove(connection.Username, out _);

                // 2. (Người 3) Xóa user khỏi tất cả các phòng
                // (Viết logic duyệt _rooms và xóa user này)

                // 3. (Người 2) Broadcast UserList MỚI
                // (Viết logic gửi UserListMessage cho tất cả user còn lại)

                Console.WriteLine($"User {connection.Username ?? "unknown"} cleaned up.");
            }
            connection.Client.Close();
        }
    }
}