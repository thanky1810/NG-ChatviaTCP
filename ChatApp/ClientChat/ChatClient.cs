// File: UI.Chat/ChatClient.cs
// (Người 5 - Nguyễn Thành Nam: Lõi Client - Networking & State)
using Chat.Shared;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ClientChat;

public class ChatClient
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;

    // (Người 5) Hàng đợi (Queue) thread-safe để luồng mạng giao tiếp với luồng UI
    private readonly BlockingCollection<BaseMessage> _inbox = new();

    public string? Username { get; private set; }

    // (Người 5) Sự kiện (Events) để báo cho UI (FormLogin, FormChat)
    public event Action<string>? ConnectionStatusChanged;
    public event Action<BaseMessage>? MessageReceived;

    // (Người 5) Hàm kết nối (UC-01)
    public async Task ConnectAsync(string host, int port, string username)
    {
        if (_client?.Connected ?? false)
            return;

        try
        {
            ConnectionStatusChanged?.Invoke("Đang kết nối...");
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
            _cts = new CancellationTokenSource();
            this.Username = username;

            // (Người 5 & 1) Gửi message Login (đã đóng gói)
            await NetworkHelpers.SendMessageAsync(_stream, new LoginMessage { Username = username });

            // (Người 5) Khởi chạy 2 luồng nền (Task)
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token)); // Luồng nhận
            _ = Task.Run(() => ConsumerLoopAsync(_cts.Token)); // Luồng xử lý hàng đợi

            ConnectionStatusChanged?.Invoke("Đã kết nối, chờ xác thực...");
        }
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke($"Kết nối thất bại: {ex.Message}");
            Disconnect();
            throw; // Ném lỗi ra để FormLogin (Người 6) bắt được
        }
    }

    // (Người 5) Luồng nhận (Receiver Thread)
    // Chỉ đọc socket, giải mã (Framing) và bỏ vào hàng đợi _inbox
    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && _stream != null)
            {
                BaseMessage message = await NetworkHelpers.ReadMessageAsync(_stream);
                _inbox.Add(message, token);
            }
        }
        catch (OperationCanceledException) { }
        catch (IOException ex) // Lỗi socket (server sập, mạng rớt)
        {
            ConnectionStatusChanged?.Invoke($"Mất kết nối: {ex.Message}");
            _inbox.CompleteAdding();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ReceiveLoop Error] {ex.Message}");
            _inbox.CompleteAdding();
        }
    }

    // (Người 5) Luồng xử lý (Consumer Thread)
    // Lấy message từ hàng đợi và bắn sự kiện MessageReceived cho UI
    private void ConsumerLoopAsync(CancellationToken token)
    {
        try
        {
            foreach (var message in _inbox.GetConsumingEnumerable(token))
            {
                MessageReceived?.Invoke(message);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            ConnectionStatusChanged?.Invoke("Đã ngắt kết nối.");
            Disconnect();
        }
    }

    // (Người 5) Hàm Gửi tin nhắn ra mạng
    public async Task SendMessageAsync(BaseMessage message)
    {
        if (_stream == null || !(_client?.Connected ?? false))
            throw new IOException("Chưa kết nối đến server.");

        // (Người 5 & 1) Dùng helper để đóng gói và gửi
        await NetworkHelpers.SendMessageAsync(_stream, message);
    }

    // (Người 5) Hàm Đóng kết nối (UC-06)
    public void Disconnect()
    {
        if (_cts == null) return;

        try
        {
            _cts?.Cancel(); // Yêu cầu 2 luồng nền dừng lại
            _stream?.Close();
            _client?.Close();
        }
        catch { }
        finally
        {
            _cts = null;
            _stream = null;
            _client = null;
            Username = null;
        }
    }
}