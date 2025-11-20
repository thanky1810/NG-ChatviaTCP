// File: Chat.Shared/Protocol.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Chat.Shared;

[JsonDerivedType(typeof(LoginMessage), typeDiscriminator: "login")]
[JsonDerivedType(typeof(LoginOkMessage), typeDiscriminator: "login_ok")]
[JsonDerivedType(typeof(ErrorMessage), typeDiscriminator: "error")]
[JsonDerivedType(typeof(LogoutMessage), typeDiscriminator: "logout")]
[JsonDerivedType(typeof(ChatPublicMessage), typeDiscriminator: "chat_public")]
[JsonDerivedType(typeof(ChatPrivateMessage), typeDiscriminator: "chat_private")]
[JsonDerivedType(typeof(ChatRoomMessage), typeDiscriminator: "chat_room")]
[JsonDerivedType(typeof(CreateRoomMessage), typeDiscriminator: "create_room")]
[JsonDerivedType(typeof(JoinRoomMessage), typeDiscriminator: "join_room")]
[JsonDerivedType(typeof(LeaveRoomMessage), typeDiscriminator: "leave_room")]
[JsonDerivedType(typeof(UserListMessage), typeDiscriminator: "user_list")]
[JsonDerivedType(typeof(RoomListMessage), typeDiscriminator: "room_list")]
[JsonDerivedType(typeof(SystemMessage), typeDiscriminator: "system")]
[JsonDerivedType(typeof(PingMessage), typeDiscriminator: "ping")]
[JsonDerivedType(typeof(PongMessage), typeDiscriminator: "pong")]

public class BaseMessage { }

// --- Nhóm Xác thực / Kết nối ---
public class LoginMessage : BaseMessage
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
}

public class LoginOkMessage : BaseMessage
{
    [JsonPropertyName("users")]
    public List<string> Users { get; set; }
    [JsonPropertyName("rooms")]
    public List<string> Rooms { get; set; }
}

public class ErrorMessage : BaseMessage
{
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public class LogoutMessage : BaseMessage { }

// --- Nhóm Chat ---
public class ChatPublicMessage : BaseMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("ts")]
    public string Timestamp { get; set; }
}

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
public class CreateRoomMessage : BaseMessage
{
    [JsonPropertyName("room")]
    public string Room { get; set; }

    // ✅ THÊM
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}

public class JoinRoomMessage : BaseMessage
{
    [JsonPropertyName("room")]
    public string Room { get; set; }

    // ✅ THÊM
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}

public class LeaveRoomMessage : BaseMessage
{
    [JsonPropertyName("room")]
    public string Room { get; set; }
}

// --- Nhóm Danh sách & Hệ thống ---
public class UserListMessage : BaseMessage
{
    [JsonPropertyName("users")]
    public List<string> Users { get; set; }
}

public class RoomListMessage : BaseMessage
{
    [JsonPropertyName("rooms")]
    public List<string> Rooms { get; set; }
}

public class SystemMessage : BaseMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class PingMessage : BaseMessage { }
public class PongMessage : BaseMessage { }