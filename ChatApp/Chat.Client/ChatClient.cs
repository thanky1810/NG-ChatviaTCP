// File: Chat.Client/ChatClient.cs
using Chat.Shared;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Chat.Client; // <--- ĐÃ ĐỔI NAMESPACE

public class ChatClient
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;
    private readonly BlockingCollection<BaseMessage> _inbox = new();

    public string? Username { get; private set; }
    public event Action<string>? ConnectionStatusChanged;
    public event Action<BaseMessage>? MessageReceived;

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

            // ✅ SỬA LỖI CS0117: Xóa "Type = ..."
            await NetworkHelpers.SendMessageAsync(_stream, new LoginMessage { Username = username });

            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
            _ = Task.Run(() => ConsumerLoopAsync(_cts.Token));

            ConnectionStatusChanged?.Invoke("Đã kết nối, chờ xác thực...");
        }
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke($"Kết nối thất bại: {ex.Message}");
            Disconnect();
            throw;
        }
    }

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
        catch (IOException ex)
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

    public async Task SendMessageAsync(BaseMessage message)
    {
        if (_stream == null || !(_client?.Connected ?? false))
            throw new IOException("Chưa kết nối đến server.");
        await NetworkHelpers.SendMessageAsync(_stream, message);
    }

    public void Disconnect()
    {
        if (_cts == null) return;
        try
        {
            _cts?.Cancel();
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