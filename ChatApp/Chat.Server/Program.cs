// File: Chat.Server/Program.cs
using Chat.Server;

Console.WriteLine("Starting Chat Server...");

// Khởi tạo và chạy Server
var server = new ChatServer("0.0.0.0", 8888); // Lắng nghe trên mọi IP, cổng 8888
await server.StartAsync();