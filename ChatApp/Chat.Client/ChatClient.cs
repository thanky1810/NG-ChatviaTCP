// File: Chat.Client/ChatClient.cs
using Chat.Shared; // <-- QUAN TRỌNG: Thêm reference
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Chat.Client;

public class ChatClient
{
    private TcpClient _client;
    private NetworkStream _stream;
    private CancellationTokenSource _cts;

    // TODO (Người 5 & 6): 
    // Dùng các Events này để báo cho GUI (WPF/WinForms) cập nhật
    public event Action<BaseMessage> MessageReceived; // Báo cho UI khi có tin nhắn mới
    public event Action<string> ConnectionStatusChanged; // Báo "Connected", "Disconnected"

    public bool IsConnected => _client?.Connected ?? false;

    // TODO (Người 6): Gọi hàm này từ nút "Connect"
    public async Task ConnectAsync(string host, int port, string username)
    {
        try
        {
            ConnectionStatusChanged?.Invoke("Connecting...");
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();

            // 1. Gửi tin nhắn Login (UC-01)
            var loginMsg = new LoginMessage { Type = "login", Username = username };
            await NetworkHelpers.SendMessageAsync(_stream, loginMsg);

            // 2. Khởi động luồng nhận (mục 6.1)
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));

            // (Chưa xong - Chúng ta phải chờ Login_OK hoặc Error từ server
            // Người 5 sẽ xử lý logic này trong ReceiveLoopAsync)
        }
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke($"Error: {ex.Message}");
            _client?.Close();
        }
    }

    // TODO (Người 5): Hoàn thiện hàm này
    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                BaseMessage message = await NetworkHelpers.ReadMessageAsync(_stream);

                // --- Quan trọng (Mục 6.1) ---
                // ĐÃ NHẬN ĐƯỢC MESSAGE TỪ LUỒNG NỀN
                // Cần đẩy nó về Luồng UI (Dispatcher)
                MessageReceived?.Invoke(message);
                // -----------------------------

                // (Gợi ý cho Người 5 & 6 xử lý login)
                if (message is LoginOkMessage)
                {
                    ConnectionStatusChanged?.Invoke("Connected!");
                    // TODO: Chuyển màn hình
                }
                else if (message is ErrorMessage errMsg && errMsg.Code == "username_taken")
                {
                    ConnectionStatusChanged?.Invoke($"Error: {errMsg.Message}");
                    Disconnect(); // Tự động ngắt kết nối
                }
            }
        }
        catch (IOException)
        {
            // Socket bị đóng (server sập, rớt mạng, hoặc ta chủ động logout)
            ConnectionStatusChanged?.Invoke("Disconnected.");
        }
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke($"Error: {ex.Message}");
        }
        finally
        {
            _client?.Close();
        }
    }

    // TODO (Người 6): Gọi hàm này từ nút "Send"
    public async Task SendMessageAsync(BaseMessage message)
    {
        if (IsConnected)
        {
            await NetworkHelpers.SendMessageAsync(_stream, message);
        }
    }

    // TODO (Người 6): Gọi hàm này từ nút "Logout" (UC-06)
    public void Disconnect()
    {
        _cts?.Cancel(); // Dừng luồng nhận
        _stream?.Close();
        _client?.Close();
        ConnectionStatusChanged?.Invoke("Disconnected.");
    }
}