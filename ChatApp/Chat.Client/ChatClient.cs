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
        }
        catch (Exception ex)
        {
            // Ngoại lệ E2: Không kết nối được
            _client?.Close();
        }
    }

    //  Hàm đã hoàn thiện (Chủ yếu là công việc của Người 5)
    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                BaseMessage message = await NetworkHelpers.ReadMessageAsync(_stream);

                // --- Đẩy message về Luồng UI (Người 6 sẽ xử lý ở đây) ---
                MessageReceived?.Invoke(message);
                // -------------------------------------------------------------

                // Logic xử lý Login/Error (Mục 6.1)
                if (message is LoginOkMessage)
                {
                    ConnectionStatusChanged?.Invoke("Connected!");
                    // Người 6 sẽ tiếp tục xử lý chuyển màn hình trong event MessageReceived.
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

    //  Hàm đã hoàn thiện: Gọi hàm này từ nút "Send" (Người 6)
    public async Task SendMessageAsync(BaseMessage message)
    {
        if (IsConnected && _stream != null)
        {
            await NetworkHelpers.SendMessageAsync(_stream, message);
        }
    }

    //  Hàm đã hoàn thiện: Gọi hàm này từ nút "Logout" (UC-06) (Người 6)
    public void Disconnect()
    {
        _cts?.Cancel(); // Dừng luồng nhận
        _stream?.Close();
        _client?.Close();
        ConnectionStatusChanged?.Invoke("Disconnected.");
    }
}