// File: Chat.Shared/NetworkHelpers.cs
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chat.Shared;

public static class NetworkHelpers
{
    // Cấu hình để nhận diện các lớp con từ lớp BaseMessage
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        TypeInfoResolver = new PolymorphicJsonTypeInfoResolver()
    };

    // --- GỬI tin (Client và Server đều dùng) ---
    // (Serializes, tính độ dài, gửi 4-byte length + payload)
    public static async Task SendMessageAsync(NetworkStream stream, BaseMessage message)
    {
        // 1. Serialize message thành JSON (UTF-8)
        // Chúng ta cần serialize bằng đúng kiểu runtime của nó (vd: LoginMessage)
        byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(message, message.GetType(), JsonOptions);

        // 2. Lấy độ dài của payload
        int length = jsonBytes.Length;

        // 3. Chuyển độ dài thành 4 byte (Big-Endian)
        byte[] lengthBytes = BitConverter.GetBytes(length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBytes); // Đảm bảo là Big-Endian
        }

        // 4. Gửi 4 byte độ dài
        await stream.WriteAsync(lengthBytes, 0, 4);

        // 5. Gửi payload (JSON)
        await stream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
    }

    // --- NHẬN tin (Client và Server đều dùng) ---
    // (Đọc 4-byte length, đọc N-byte payload)
    // Trả về đối tượng BaseMessage đã được deserialize
    public static async Task<BaseMessage> ReadMessageAsync(NetworkStream stream)
    {
        byte[] lengthBuffer = new byte[4];

        // 1. Đọc chính xác 4 byte đầu tiên (Length)
        int bytesRead = await ReadExactBytesAsync(stream, lengthBuffer, 4);
        if (bytesRead < 4)
            throw new IOException("Socket bị đóng đột ngột khi đang đọc độ dài.");

        // 2. Chuyển 4 byte (Big-Endian) về số int
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBuffer);
        }
        int length = BitConverter.ToInt32(lengthBuffer, 0);

        // 3. Đọc chính xác N byte (Payload)
        byte[] payloadBuffer = new byte[length];
        bytesRead = await ReadExactBytesAsync(stream, payloadBuffer, length);
        if (bytesRead < length)
            throw new IOException("Socket bị đóng đột ngột khi đang đọc payload.");

        // 4. Deserialize payload (UTF-8) về BaseMessage
        // System.Text.Json sẽ tự động nhận diện đúng kiểu (Login, Chat, etc.)
        // nhờ vào thuộc tính "type" và cấu hình JsonOptions
        var message = JsonSerializer.Deserialize<BaseMessage>(payloadBuffer, JsonOptions);

        if (message == null)
            throw new JsonException("Không thể deserialize message.");

        return message;
    }

    // Hàm helper để đảm bảo đọc đủ số byte yêu cầu
    private static async Task<int> ReadExactBytesAsync(NetworkStream stream, byte[] buffer, int bytesToRead)
    {
        int totalBytesRead = 0;
        while (totalBytesRead < bytesToRead)
        {
            int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, bytesToRead - totalBytesRead);
            if (bytesRead == 0) // Socket đã bị đóng
                return totalBytesRead;
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }
}