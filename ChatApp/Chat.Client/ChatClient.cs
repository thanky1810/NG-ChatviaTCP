// File: Chat.Client/ChatClient.cs
using Chat.Shared; // <-- QUAN TRỌNG: Thêm reference
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Chat.Client;

public class ChatClient : IDisposable
{
    private TcpClient _client;
    private NetworkStream _stream;
    private CancellationTokenSource _cts;
    private readonly object _sync = new object();

    // Hàng đợi nhận tin (thread-safe). Receiver thread đẩy vào đây,
    // UI sẽ tiêu thụ (poll hoặc start consumer) và cập nhật UI qua dispatcher.
    private readonly BlockingCollection<BaseMessage> _inbox = new BlockingCollection<BaseMessage>();

    // Sự kiện để GUI (WPF/WinForms) đăng ký lắng nghe (tùy chọn)
    public event Action<BaseMessage> MessageReceived; // Báo cho UI khi có tin nhắn mới (tùy chọn)
    public event Action<string> ConnectionStatusChanged; // Báo trạng thái kết nối: "Connected", "Disconnected", "Error", ...

    public bool IsConnected => _client?.Connected ?? false;

    // Kết nối và gửi Login (giữ nguyên)
    public async Task ConnectAsync(string host, int port, string username)
    {
        try
        {
            ConnectionStatusChanged?.Invoke("Connecting...");
            _client = new TcpClient();
            await _client.ConnectAsync(host, port).ConfigureAwait(false);
            lock (_sync)
            {
                _stream = _client.GetStream();
            }

            // Gửi Login (dùng framing trong NetworkHelpers)
            var loginMsg = new LoginMessage { Type = "login", Username = username };
            await SendFramedAsync(loginMsg).ConfigureAwait(false);

            // Khởi động receiver loop (background Task)
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token), CancellationToken.None);

            ConnectionStatusChanged?.Invoke("Connected (login sent)");
        }
        catch (Exception ex)
        {
            try { _client?.Close(); } catch { }
            ConnectionStatusChanged?.Invoke($"Error: {ex.Message}");
            throw;
        }
    }

    // Receiver loop:
    // - Liên tục đọc message (NetworkHelpers.ReadMessageAsync đã làm framing + JSON parse)
    // - Đẩy message vào _inbox (BlockingCollection) để consumer/ UI xử lý sau
    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                NetworkStream streamSnapshot;
                lock (_sync) { streamSnapshot = _stream; }

                if (streamSnapshot == null)
                {
                    ConnectionStatusChanged?.Invoke("Disconnected.");
                    break;
                }

                // ReadMessageAsync thực hiện: đọc 4-byte length + payload, deserialize JSON -> BaseMessage
                BaseMessage message = await NetworkHelpers.ReadMessageAsync(streamSnapshot).ConfigureAwait(false);

                // Đẩy vào hàng đợi (non-blocking với token)
                try
                {
                    // Nếu token đã cancel, sẽ throw OperationCanceledException khi Add với token
                    _inbox.Add(message, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                // Vẫn giữ event tùy chọn (UI có thể dùng event thay vì poll)
                MessageReceived?.Invoke(message);

                // Xử lý trạng thái cơ bản
                if (message is LoginOkMessage)
                {
                    ConnectionStatusChanged?.Invoke("Connected!");
                }
                else if (message is ErrorMessage errMsg && errMsg.Code == "username_taken")
                {
                    ConnectionStatusChanged?.Invoke($"Error: {errMsg.Message}");
                    Disconnect();
                    break;
                }
            }
        }
        catch (IOException)
        {
            ConnectionStatusChanged?.Invoke("Disconnected.");
        }
        catch (OperationCanceledException)
        {
            // bình thường khi hủy
            ConnectionStatusChanged?.Invoke("Receive loop cancelled.");
        }
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke($"Error: {ex.Message}");
        }
        finally
        {
            // Đóng kết nối và đánh dấu không còn item thêm vào inbox
            try { _client?.Close(); } catch { }
            _inbox.CompleteAdding();
        }
    }

    // PUBLIC: gửi message (sử dụng framing)
    public async Task SendMessageAsync(BaseMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        await SendFramedAsync(message).ConfigureAwait(false);
    }

    // Internal: gọi NetworkHelpers để framing + gửi
    private async Task SendFramedAsync(BaseMessage message, CancellationToken cancellation = default)
    {
        NetworkStream streamSnapshot;
        lock (_sync) { streamSnapshot = _stream; }

        if (streamSnapshot == null || !_client?.Connected == true)
            throw new InvalidOperationException("Chưa kết nối tới server.");

        await NetworkHelpers.SendMessageAsync(streamSnapshot, message).ConfigureAwait(false);
    }

    // Ngắt kết nối: hủy receiver, đóng stream/socket, đánh dấu hoàn tất inbox
    public void Disconnect()
    {
        _cts?.Cancel();
        try { lock (_sync) { _stream?.Close(); } } catch { }
        try { _client?.Close(); } catch { }

        // Khi đóng, kết thúc adding để các consumer biết không còn dữ liệu mới
        _inbox.CompleteAdding();

        ConnectionStatusChanged?.Invoke("Disconnected.");
    }

    // Wrapper Connect non-throwing
    public async Task<bool> Connect(string host, int port, string username)
    {
        try
        {
            await ConnectAsync(host, port, username).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke($"Error: {ex.Message}");
            return false;
        }
    }

    // Consumer helpers

    // 1) Lấy một message nếu có (non-blocking)
    public bool TryTakeMessage(out BaseMessage message)
    {
        return _inbox.TryTake(out message);
    }

    // 2) Lấy một message với timeout (ms)
    public bool TryTakeMessage(int millisecondsTimeout, out BaseMessage message)
    {
        return _inbox.TryTake(out message, millisecondsTimeout);
    }

    // 3) Lấy message blocking (sử dụng CancellationToken)
    public BaseMessage TakeMessage(CancellationToken cancellation = default)
    {
        return _inbox.Take(cancellation);
    }

    // 4) Bắt đầu consumer async: truyền delegate onMessage (UI nên marshal vào UI thread)
    //    Trả Task cho phép UI chờ/huỷ nếu cần.
    public Task StartConsumerAsync(Action<BaseMessage> onMessage, CancellationToken cancellation = default)
    {
        if (onMessage == null) throw new ArgumentNullException(nameof(onMessage));

        return Task.Run(() =>
        {
            try
            {
                foreach (var msg in _inbox.GetConsumingEnumerable(cancellation))
                {
                    try
                    {
                        onMessage(msg);
                    }
                    catch
                    {
                        // Không để exception từ handler làm dừng consumer
                    }

                    if (cancellation.IsCancellationRequested)
                        break;
                }
            }
            catch (OperationCanceledException) { /* ok */ }
        }, CancellationToken.None);
    }

    public void Dispose()
    {
        Disconnect();
        try { _cts?.Dispose(); } catch { }
        // đảm bảo release blocking collection
        _inbox.Dispose();
    }
}