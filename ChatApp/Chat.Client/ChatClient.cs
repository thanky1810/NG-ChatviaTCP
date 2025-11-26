// File: Chat.Client/ChatClient.cs
// (Người 5 - Nguyễn Thành Nam: Lõi Client - Networking & State)
using Chat.Shared;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Chat.Client;

public class ChatClient
{
    // (Người 5) Các biến quản lý kết nối và luồng
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;

    // (Người 5) Hàng đợi tin nhắn thread-safe
    private readonly BlockingCollection<BaseMessage> _inbox = new();

    public string? Username { get; private set; }

    // (Người 5) Sự kiện thông báo trạng thái cho GUI
    public event Action<string>? ConnectionStatusChanged;
    public event Action<BaseMessage>? MessageReceived;

    // (Người 5) Hàm kết nối đến Server
    public async Task ConnectAsync(string host, int port, string username)
    {
        if (_client?.Connected ?? false) return;

        try
        {
            ConnectionStatusChanged?.Invoke("Đang kết nối...");
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
            _cts = new CancellationTokenSource();
            this.Username = username;

            // (Người 5) Gửi tin nhắn đăng nhập
            await NetworkHelpers.SendMessageAsync(_stream, new LoginMessage { Username = username });

            // (Người 5) Khởi chạy các luồng nền xử lý mạng
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
            _ = Task.Run(() => ConsumerLoopAsync(_cts.Token));
            _ = Task.Run(() => PingLoopAsync(_cts.Token));

            ConnectionStatusChanged?.Invoke("Đã kết nối.");
        }
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke($"Lỗi: {ex.Message}");
            Disconnect();
            throw;
        }
    }

    // (Người 5) Luồng gửi Ping định kỳ (Heartbeat)
    private async Task PingLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _stream != null)
        {
            try
            {
                await Task.Delay(30000, token);
                await NetworkHelpers.SendMessageAsync(_stream, new PingMessage());
            }
            catch { break; }
        }
    }

    // (Người 5) Luồng nhận dữ liệu từ Server
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
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke($"Mất kết nối: {ex.Message}");
            _inbox.CompleteAdding();
        }
    }

    // (Người 5) Luồng xử lý tin nhắn và gửi sự kiện ra ngoài
    private void ConsumerLoopAsync(CancellationToken token)
    {
        try { foreach (var msg in _inbox.GetConsumingEnumerable(token)) MessageReceived?.Invoke(msg); }
        catch { }
        finally { Disconnect(); }
    }

    // (Người 5) Hàm gửi tin nhắn
    public async Task SendMessageAsync(BaseMessage message)
    {
        if (_stream == null) throw new IOException("Disconnected");
        await NetworkHelpers.SendMessageAsync(_stream, message);
    }

    // (Người 5) Hàm ngắt kết nối và dọn dẹp tài nguyên
    public void Disconnect()
    {
        if (_cts == null) return;
        try { _cts.Cancel(); _stream?.Close(); _client?.Close(); }
        catch { }
        finally { _cts = null; _stream = null; _client = null; Username = null; }
    }
}