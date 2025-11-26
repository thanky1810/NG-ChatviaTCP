// File: Chat.Server/ServerForm.cs
// (Người 1 - Vũ Trí Dũng: Logic Giao diện Server Dashboard)
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

        // (Người 1: Khởi tạo lõi Server)
        _server = new ChatServer("0.0.0.0", 8888);

        // (Người 1: Đăng ký nhận sự kiện từ lõi)
        _server.LogMessageReceived += OnLogMessageReceived;
        _server.UserListChanged += OnUserListChanged;
        _server.RoomListChanged += OnRoomListChanged;

        // (Người 1: Đảm bảo tắt process khi đóng form)
        this.FormClosing += ServerForm_FormClosing;
    }

    private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Environment.Exit(0);
    }

    private void ServerForm_Load(object sender, EventArgs e)
    {
        // (Người 1: Chạy Server trên luồng nền)
        Task.Run(() => _server.StartAsync());
    }

    // (Người 1: Cập nhật Log UI)
    private void OnLogMessageReceived(string message)
    {
        if (rtbLogs.InvokeRequired)
        {
            rtbLogs.BeginInvoke((Action)(() => OnLogMessageReceived(message)));
            return;
        }

        Color logColor = Color.Black;
        if (message.StartsWith("[ERROR]")) logColor = Color.Red;
        else if (message.StartsWith("[WARN]")) logColor = Color.Goldenrod;
        else if (message.StartsWith("[INFO]")) logColor = Color.DarkGreen;

        rtbLogs.SelectionStart = rtbLogs.TextLength;
        rtbLogs.SelectionColor = logColor;
        rtbLogs.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
        rtbLogs.ScrollToCaret();
    }

    // (Người 2: Cập nhật danh sách User UI)
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

    // (Người 3: Cập nhật danh sách Room UI)
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