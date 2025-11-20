// File: UI.Chat/ChatClient.cs
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
    private readonly BlockingCollection<BaseMessage> _inbox = new();

    public string? Username { get; private set; }
    public event Action<string>? ConnectionStatusChanged;
    public event Action<BaseMessage>? MessageReceived;

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

            await NetworkHelpers.SendMessageAsync(_stream, new LoginMessage { Username = username });

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

    private void ConsumerLoopAsync(CancellationToken token)
    {
        try { foreach (var msg in _inbox.GetConsumingEnumerable(token)) MessageReceived?.Invoke(msg); }
        catch { }
        finally { Disconnect(); }
    }

    public async Task SendMessageAsync(BaseMessage message)
    {
        if (_stream == null) throw new IOException("Disconnected");
        await NetworkHelpers.SendMessageAsync(_stream, message);
    }

    public void Disconnect()
    {
        if (_cts == null) return;
        try { _cts.Cancel(); _stream?.Close(); _client?.Close(); }
        catch { }
        finally { _cts = null; _stream = null; _client = null; Username = null; }
    }
}