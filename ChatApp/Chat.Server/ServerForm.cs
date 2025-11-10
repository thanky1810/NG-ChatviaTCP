// File: Chat.Server/ServerForm.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat.Server;

public partial class ServerForm : Form
{
    private readonly ChatServer _server;

    public ServerForm()
    {
        InitializeComponent();

        // 1. Khởi tạo lõi Server (nhưng chưa chạy)
        _server = new ChatServer("0.0.0.0", 8888);

        // 2. Đăng ký nhận sự kiện từ lõi Server
        _server.LogMessageReceived += OnLogMessageReceived;
        _server.UserListChanged += OnUserListChanged;
        _server.RoomListChanged += OnRoomListChanged;
    }

    private void ServerForm_Load(object sender, EventArgs e)
    {
        // 3. Khởi chạy Server trên một luồng nền
        //    (Để nó không làm "đóng băng" Giao diện)
        Task.Run(() => _server.StartAsync());
    }

    // --- Các hàm cập nhật UI (Thread-safe) ---
    // (Những hàm này được gọi từ luồng mạng của Server)

    private void OnLogMessageReceived(string message)
    {
        // Phải dùng BeginInvoke để cập nhật UI từ luồng khác
        if (rtbLogs.InvokeRequired)
        {
            rtbLogs.BeginInvoke((Action)(() => OnLogMessageReceived(message)));
            return;
        }

        // Tô màu cho log
        Color logColor = Color.Black;
        if (message.StartsWith("[ERROR]")) logColor = Color.Red;
        else if (message.StartsWith("[WARN]")) logColor = Color.Goldenrod;
        else if (message.StartsWith("[INFO]")) logColor = Color.DarkGreen;

        rtbLogs.SelectionStart = rtbLogs.TextLength;
        rtbLogs.SelectionColor = logColor;
        rtbLogs.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
        rtbLogs.ScrollToCaret();
    }

    private void OnUserListChanged(List<string> users)
    {
        if (lboxUsers.InvokeRequired)
        {
            lboxUsers.BeginInvoke((Action)(() => OnUserListChanged(users)));
            return;
        }

        lboxUsers.Items.Clear();
        foreach (var user in users)
        {
            lboxUsers.Items.Add(user);
        }
        lblUserCount.Text = $"Users: {users.Count}";
    }

    private void OnRoomListChanged(List<string> rooms)
    {
        if (lboxRooms.InvokeRequired)
        {
            lboxRooms.BeginInvoke((Action)(() => OnRoomListChanged(rooms)));
            return;
        }

        lboxRooms.Items.Clear();
        foreach (var room in rooms)
        {
            lboxRooms.Items.Add(room);
        }
    }
}