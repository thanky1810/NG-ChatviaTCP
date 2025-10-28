// File: Chat.Shared/Protocol.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Chat.Shared;

// Lớp cơ sở để xác định loại thông điệp
public class BaseMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

// --- Nhóm Xác thực / Kết nối ---

[JsonDerivedType(typeof(LoginMessage), typeDiscriminator: "login")]
public class LoginMessage : BaseMessage
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
}

[JsonDerivedType(typeof(LoginOkMessage), typeDiscriminator: "login_ok")]
public class LoginOkMessage : BaseMessage
{
    [JsonPropertyName("users")]
    public List<string> Users { get; set; }
    [JsonPropertyName("rooms")]
    public List<string> Rooms { get; set; }
}

[JsonDerivedType(typeof(ErrorMessage), typeDiscriminator: "error")]
public class ErrorMessage : BaseMessage
{
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
}

[JsonDerivedType(typeof(LogoutMessage), typeDiscriminator: "logout")]
public class LogoutMessage : BaseMessage
{
    // Không cần thêm trường nào cho logout
}

// --- Nhóm Chat ---

[JsonDerivedType(typeof(ChatPublicMessage), typeDiscriminator: "chat_public")]
public class ChatPublicMessage : BaseMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("ts")]
    public string Timestamp { get; set; } // Dùng string cho ISO-8601
}

[JsonDerivedType(typeof(ChatPrivateMessage), typeDiscriminator: "chat_private")]
public class ChatPrivateMessage : BaseMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; }
    [JsonPropertyName("to")]
    public string To { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("ts")]
    public string Timestamp { get; set; }
}

[JsonDerivedType(typeof(ChatRoomMessage), typeDiscriminator: "chat_room")]
public class ChatRoomMessage : BaseMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; }
    [JsonPropertyName("room")]
    public string Room { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("ts")]
    public string Timestamp { get; set; }
}

// --- Nhóm Phòng (Rooms) ---

[JsonDerivedType(typeof(CreateRoomMessage), typeDiscriminator: "create_room")]
public class CreateRoomMessage : BaseMessage
{
    [JsonPropertyName("room")]
    public string Room { get; set; }
}

[JsonDerivedType(typeof(JoinRoomMessage), typeDiscriminator: "join_room")]
public class JoinRoomMessage : BaseMessage
{
    [JsonPropertyName("room")]
    public string Room { get; set; }
}

[JsonDerivedType(typeof(LeaveRoomMessage), typeDiscriminator: "leave_room")]
public class LeaveRoomMessage : BaseMessage
{
    [JsonPropertyName("room")]
    public string Room { get; set; }
}

// --- Nhóm Danh sách & Hệ thống ---

[JsonDerivedType(typeof(UserListMessage), typeDiscriminator: "user_list")]
public class UserListMessage : BaseMessage
{
    [JsonPropertyName("users")]
    public List<string> Users { get; set; }
}

[JsonDerivedType(typeof(RoomListMessage), typeDiscriminator: "room_list")]
public class RoomListMessage : BaseMessage
{
    [JsonPropertyName("rooms")]
    public List<string> Rooms { get; set; }
}

[JsonDerivedType(typeof(SystemMessage), typeDiscriminator: "system")]
public class SystemMessage : BaseMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

// (Tùy chọn) Heartbeat
[JsonDerivedType(typeof(PingMessage), typeDiscriminator: "ping")]
public class PingMessage : BaseMessage { }

[JsonDerivedType(typeof(PongMessage), typeDiscriminator: "pong")]
public class PongMessage : BaseMessage { }