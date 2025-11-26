// File: Chat.Shared/NetworkHelpers.cs
// (Người 1 - Vũ Trí Dũng: Xử lý Đóng gói (Framing) và Truyền nhận dữ liệu)
using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chat.Shared;

public static class NetworkHelpers
{
    // (Người 1) Cấu hình Serializer để hỗ trợ đa hình
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        TypeInfoResolver = new PolymorphicJsonTypeInfoResolver()
    };

    // (Người 1) Hàm gửi tin nhắn (Framing: 4-byte Length + Payload)
    public static async Task SendMessageAsync(NetworkStream stream, BaseMessage message)
    {
        // 1. Serialize message
        byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);

        // 2. Tính độ dài payload
        int length = jsonBytes.Length;

        // 3. Chuyển độ dài thành 4 byte (Big-Endian)
        byte[] lengthBytes = BitConverter.GetBytes(length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBytes);
        }

        // 4. Gửi độ dài trước
        await stream.WriteAsync(lengthBytes, 0, 4);

        // 5. Gửi nội dung sau
        await stream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
    }

    // (Người 1) Hàm nhận tin nhắn (Giải mã Length-Prefix)
    public static async Task<BaseMessage> ReadMessageAsync(NetworkStream stream)
    {
        byte[] lengthBuffer = new byte[4];

        // 1. Đọc 4 byte đầu tiên (Length)
        int bytesRead = await ReadExactBytesAsync(stream, lengthBuffer, 4);
        if (bytesRead < 4)
            throw new IOException("Socket closed or invalid header.");

        // 2. Chuyển đổi về số nguyên (Big-Endian)
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBuffer);
        }
        int length = BitConverter.ToInt32(lengthBuffer, 0);

        if (length > 1_048_576) throw new IOException($"Payload too large: {length} bytes.");
        if (length < 2) throw new IOException("Payload invalid.");

        // 3. Đọc phần nội dung (Payload) dựa trên độ dài
        byte[] payloadBuffer = new byte[length];
        bytesRead = await ReadExactBytesAsync(stream, payloadBuffer, length);
        if (bytesRead < length)
            throw new IOException("Socket closed while reading payload.");

        // 4. Deserialize về đối tượng C#
        var message = JsonSerializer.Deserialize<BaseMessage>(payloadBuffer, JsonOptions);
        if (message == null) throw new JsonException("Deserialization failed.");

        return message;
    }

    // (Người 1) Hàm helper để đảm bảo đọc đủ số byte yêu cầu
    private static async Task<int> ReadExactBytesAsync(NetworkStream stream, byte[] buffer, int bytesToRead)
    {
        int totalBytesRead = 0;
        while (totalBytesRead < bytesToRead)
        {
            int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, bytesToRead - totalBytesRead);
            if (bytesRead == 0) return totalBytesRead;
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }
}