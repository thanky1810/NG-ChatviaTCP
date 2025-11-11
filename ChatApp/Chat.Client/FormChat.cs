// File: Chat.Client/FormChat.cs
// (Người 4 - Nguyễn Thị Hoài Linh: Logic Màn hình Chat chính)
// (Người 6 - Cao Xuân Quyết: Tích hợp Gửi tin, Rooms, Logout)
using Chat.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Chat.Client; // <-- Namespace của Project này

public partial class Chat_TCP_Client : Form
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ChatClient Client { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string UserName { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public LoginOkMessage? InitialLoginOk { get; set; }

    private CancellationTokenSource _cts;
    private enum ChatContext { Public, Private, Room }
    private ChatContext _currentContext = ChatContext.Public;
    private string _currentContextTarget = "public";

    // (Người 4) Cải tiến: Bộ nhớ đệm (Dictionary) để lưu lịch sử chat
    private Dictionary<string, string> _chatHistories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public Chat_TCP_Client()
    {
        InitializeComponent();
        this.Load += Chat_TCP_Client_Load;
    }

    private void Chat_TCP_Client_Load(object sender, EventArgs e)
    {
        this.Text = $"Chat - {UserName}";
        _cts = new CancellationTokenSource();
        Client.MessageReceived += ProcessMessage;
        UpdateChatContext(ChatContext.Public, "public");

        if (InitialLoginOk != null)
        {
            ProcessUserList(InitialLoginOk.Users);
            ProcessRoomList(InitialLoginOk.Rooms);
        }
    }

    private void ProcessUserList(List<string> users)
    {
        lboxUsers.Items.Clear();
        foreach (var user in users)
        {
            if (user != this.UserName) lboxUsers.Items.Add(user);
        }
    }

    private void ProcessRoomList(List<string> rooms)
    {
        lboxRooms.Items.Clear();
        foreach (var room in rooms)
            lboxRooms.Items.Add(room);
    }

    // (Người 4) Xử lý message nhận được
    private void ProcessMessage(BaseMessage message)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke((Action)(() => ProcessMessage(message)));
            return;
        }

        string messageContext = "";
        string sender = "??";
        string text = "";
        string ts = DateTime.UtcNow.ToString("o");

        switch (message)
        {
            case ChatPublicMessage chat:
                if (chat.From == this.UserName) return;
                messageContext = "public";
                sender = chat.From; text = chat.Text; ts = chat.Timestamp;
                break;
            case ChatPrivateMessage dm:
                if (dm.From == this.UserName) return;
                messageContext = dm.From;
                sender = dm.From; text = dm.Text; ts = dm.Timestamp;
                break;
            case ChatRoomMessage roomChat:
                if (roomChat.From == this.UserName) return;
                messageContext = roomChat.Room;
                sender = roomChat.From; text = roomChat.Text; ts = roomChat.Timestamp;
                break;

            case SystemMessage sys:
                AppendChatMessage(ts, "Hệ thống", sys.Text, false, Color.DarkGoldenrod);
                return;
            case ErrorMessage err:
                AppendChatMessage(ts, "Lỗi", err.Message, false, Color.Red);
                return;

            case UserListMessage userList:
                ProcessUserList(userList.Users);
                return;
            case RoomListMessage roomList:
                ProcessRoomList(roomList.Rooms);
                return;
        }

        if (messageContext == _currentContextTarget)
        {
            AppendChatMessage(ts, sender, text, false);
        }
        else if (!string.IsNullOrEmpty(messageContext))
        {
            // (Người 4) Cải tiến: Lưu vào lịch sử (dictionary)
            string oldRtf = _chatHistories.ContainsKey(messageContext) ? _chatHistories[messageContext] : "";
            using (var tempRtb = new RichTextBox())
            {
                if (!string.IsNullOrEmpty(oldRtf)) tempRtb.Rtf = oldRtf;

                string time = DateTime.TryParse(ts, out var dt) ? dt.ToLocalTime().ToString("HH:mm:ss") : DateTime.Now.ToString("HH:mm:ss");
                tempRtb.SelectionStart = tempRtb.TextLength;
                tempRtb.SelectionColor = Color.Gray;
                tempRtb.AppendText($"{time} ");
                tempRtb.SelectionColor = Color.Purple;
                tempRtb.AppendText($"{sender}: ");
                tempRtb.SelectionColor = Color.Black;
                tempRtb.AppendText($"{text}{Environment.NewLine}");

                _chatHistories[messageContext] = tempRtb.Rtf;
            }
        }
    }

    // (Người 6) Tích hợp Gửi tin
    private async void btnSend_Click(object sender, EventArgs e)
    {
        string text = txtMessInput.Text;
        if (string.IsNullOrWhiteSpace(text) || Client == null) return;

        BaseMessage message;
        switch (_currentContext)
        {
            case ChatContext.Public:
                // ✅ SỬA LỖI CS0117: Đã xóa "Type = ..."
                message = new ChatPublicMessage { Text = text };
                break;
            case ChatContext.Private:
                // ✅ SỬA LỖI CS0117: Đã xóa "Type = ..."
                message = new ChatPrivateMessage { To = _currentContextTarget, Text = text };
                break;
            case ChatContext.Room:
                // ✅ SỬA LỖI CS0117: Đã xóa "Type = ..."
                message = new ChatRoomMessage { Room = _currentContextTarget, Text = text };
                break;
            default:
                return;
        }

        // (Người 4) Hiển thị tin nhắn của chính mình
        AppendChatMessage(DateTime.UtcNow.ToString("o"), this.UserName, text, true);

        try
        {
            await Client.SendMessageAsync(message);
            txtMessInput.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gửi thất bại: {ex.Message}");
        }
    }

    // (Người 6) Tích hợp Tạo phòng
    private async void btnCreate_Click(object sender, EventArgs e)
    {
        using (var dlg = new FormCreate())
        {
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                var roomName = dlg.RoomName;
                if (string.IsNullOrEmpty(roomName)) return;
                // ✅ SỬA LỖI CS0117: Đã xóa "Type = ..."
                await Client.SendMessageAsync(new CreateRoomMessage { Room = roomName });
            }
        }
    }

    // (Người 6) Tích hợp Vào phòng
    private async void btnJoin_Click(object sender, EventArgs e)
    {
        if (lboxRooms.SelectedItem == null)
        {
            MessageBox.Show("Vui lòng chọn một phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var roomName = lboxRooms.SelectedItem.ToString();
        // ✅ SỬA LỖI CS0117: Đã xóa "Type = ..."
        await Client.SendMessageAsync(new JoinRoomMessage { Room = roomName });
        UpdateChatContext(ChatContext.Room, roomName);
    }

    // (Người 4) Xử lý Double-click user
    private void lboxUsers_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lboxUsers.SelectedItem == null) return;
        var username = lboxUsers.SelectedItem.ToString();
        UpdateChatContext(ChatContext.Private, username);
    }

    // (Người 6) Tích hợp Rời phòng
    private async void btnLeave_Click(object sender, EventArgs e)
    {
        if (_currentContext != ChatContext.Room)
        {
            MessageBox.Show("Bạn không ở trong phòng nào.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // ✅ SỬA LỖI CS0117: Đã xóa "Type = ..."
        await Client.SendMessageAsync(new LeaveRoomMessage { Room = _currentContextTarget });
        UpdateChatContext(ChatContext.Public, "public");
    }

    // (Người 6) Tích hợp Logout
    private async void btnLogOut_Click(object sender, EventArgs e)
    {
        var confirm = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            // ✅ SỬA LỖI CS0117: Đã xóa "Type = ..."
            await Client.SendMessageAsync(new LogoutMessage());
        }
        catch { }

        Client.Disconnect();
        _cts.Cancel();
        this.Close();
    }

    // (Người 4) Cải tiến: Hàm LƯU và TẢI lịch sử chat
    private void UpdateChatContext(ChatContext context, string target)
    {
        if (!string.IsNullOrEmpty(_currentContextTarget))
        {
            _chatHistories[_currentContextTarget] = rtbMessList.Rtf;
        }

        _currentContext = context;
        _currentContextTarget = target;

        if (_chatHistories.ContainsKey(target))
        {
            rtbMessList.Rtf = _chatHistories[target];
        }
        else
        {
            rtbMessList.Clear();
        }

        switch (context)
        {
            case ChatContext.Public:
                lblNameRoom.Text = "Chat Công Khai";
                btnLeave.Enabled = false;
                break;
            case ChatContext.Private:
                lblNameRoom.Text = $"Chat riêng: {target}";
                btnLeave.Enabled = false;
                break;
            case ChatContext.Room:
                lblNameRoom.Text = $"Phòng: {target}";
                btnLeave.Enabled = true;
                break;
        }

        rtbMessList.ScrollToCaret();
    }

    // (Người 4) Hàm helper để thêm tin nhắn vào UI
    private void AppendChatMessage(string timeStr, string sender, string message, bool isSelf, Color? senderColor = null)
    {
        string time = DateTime.TryParse(timeStr, out var dt)
            ? dt.ToLocalTime().ToString("HH:mm:ss")
            : DateTime.Now.ToString("HH:mm:ss");

        rtbMessList.SelectionStart = rtbMessList.TextLength;
        rtbMessList.ScrollToCaret();

        rtbMessList.SelectionColor = Color.Gray;
        rtbMessList.AppendText($"{time} ");

        rtbMessList.SelectionColor = senderColor ?? (isSelf ? Color.Blue : Color.Purple);
        rtbMessList.AppendText($"{sender}: ");

        rtbMessList.SelectionColor = Color.Black;
        rtbMessList.AppendText($"{message}{Environment.NewLine}");

        if (!string.IsNullOrEmpty(_currentContextTarget))
        {
            _chatHistories[_currentContextTarget] = rtbMessList.Rtf;
        }
    }

    #region (Người 4 & 6 - Các hàm UI phụ trợ)
    private void Chat_TCP_Client_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (Client?.Username != null)
        {
            DialogResult result = MessageBox.Show(
               "Đang ngắt kết nối... Bạn có chắc muốn thoát?",
               "Confirm Leave",
               MessageBoxButtons.YesNo,
               MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                Client?.Disconnect();
                _cts?.Cancel();
            }
        }
    }
    private void lboxRooms_SelectedIndexChanged(object sender, EventArgs e) { }
    private void rtbMessList_TextChanged(object sender, EventArgs e) { }
    private void panel1_Paint(object sender, EventArgs e) { }
    private void Connect_Click(object sender, EventArgs e) { }
    private void label1_Click(object sender, EventArgs e) { }
    private void txtMessageInput_TextChanged(object sender, EventArgs e) { }
    private void pnlMessageList_Paint(object sender, PaintEventArgs e) { }
    private void pnlChatFame_Paint(object sender, PaintEventArgs e) { }
    private void pnlChatHeader_Paint(object sender, EventArgs e) { }
    #endregion
}