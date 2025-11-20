// File: UI.Chat/FormChat.cs
using Chat.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ClientChat;

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
        if (Client != null) Client.MessageReceived += ProcessMessage;
        UpdateChatContext(ChatContext.Public, "public");
        if (InitialLoginOk != null) { ProcessUserList(InitialLoginOk.Users); ProcessRoomList(InitialLoginOk.Rooms); }
    }

    private void ProcessUserList(List<string> users)
    {
        lboxUsers.Items.Clear();
        foreach (var user in users) if (user != this.UserName) lboxUsers.Items.Add(user);
    }
    private void ProcessRoomList(List<string> rooms)
    {
        lboxRooms.Items.Clear();
        foreach (var room in rooms) lboxRooms.Items.Add(room);
    }

    private void ProcessMessage(BaseMessage message)
    {
        if (this.InvokeRequired) { this.BeginInvoke((Action)(() => ProcessMessage(message))); return; }

        string messageContext = ""; string sender = "??"; string text = ""; string ts = DateTime.UtcNow.ToString("o");

        switch (message)
        {
            case PongMessage:
                AppendChatMessage(DateTime.Now.ToString("HH:mm:ss"), "Server", "PONG! (Kết nối tốt)", false, Color.Green);
                return;

            case ChatPublicMessage chat:
                if (chat.From == this.UserName) return;
                messageContext = "public"; sender = chat.From; text = chat.Text; ts = chat.Timestamp;
                break;
            case ChatPrivateMessage dm:
                if (dm.From == this.UserName) return;
                messageContext = dm.From; sender = dm.From; text = dm.Text; ts = dm.Timestamp;
                break;
            case ChatRoomMessage roomChat:
                if (roomChat.From == this.UserName) return;
                messageContext = roomChat.Room; sender = roomChat.From; text = roomChat.Text; ts = roomChat.Timestamp;
                break;

            case SystemMessage sys: AppendChatMessage(ts, "Hệ thống", sys.Text, false, Color.DarkGoldenrod); return;
            case ErrorMessage err: AppendChatMessage(ts, "Lỗi", err.Message, false, Color.Red); return;

            case UserListMessage u: ProcessUserList(u.Users); return;
            case RoomListMessage r: ProcessRoomList(r.Rooms); return;
        }

        if (messageContext == _currentContextTarget) AppendChatMessage(ts, sender, text, false);
        else if (!string.IsNullOrEmpty(messageContext))
        {
            string oldRtf = _chatHistories.ContainsKey(messageContext) ? _chatHistories[messageContext] : "";
            using (var tempRtb = new RichTextBox())
            {
                if (!string.IsNullOrEmpty(oldRtf)) tempRtb.Rtf = oldRtf;
                string time = DateTime.TryParse(ts, out var dt) ? dt.ToLocalTime().ToString("HH:mm:ss") : DateTime.Now.ToString("HH:mm:ss");
                tempRtb.SelectionStart = tempRtb.TextLength; tempRtb.SelectionColor = Color.Gray; tempRtb.AppendText($"{time} ");
                tempRtb.SelectionColor = Color.Purple; tempRtb.AppendText($"{sender}: ");
                tempRtb.SelectionColor = Color.Black; tempRtb.AppendText($"{text}{Environment.NewLine}");
                _chatHistories[messageContext] = tempRtb.Rtf;
            }
        }
    }

    private async void btnPing_Click(object sender, EventArgs e)
    {
        if (Client == null) return;
        AppendChatMessage(DateTime.Now.ToString("HH:mm:ss"), "Hệ thống", "Đang gửi Ping...", false, Color.Gray);
        try { await Client.SendMessageAsync(new PingMessage()); } catch { }
    }

    private async void btnSend_Click(object sender, EventArgs e)
    {
        string text = txtMessInput.Text;
        if (string.IsNullOrWhiteSpace(text) || Client == null) return;

        BaseMessage message;
        switch (_currentContext)
        {
            case ChatContext.Public: message = new ChatPublicMessage { Text = text }; break;
            case ChatContext.Private: message = new ChatPrivateMessage { To = _currentContextTarget, Text = text }; break;
            case ChatContext.Room: message = new ChatRoomMessage { Room = _currentContextTarget, Text = text }; break;
            default: return;
        }
        AppendChatMessage(DateTime.UtcNow.ToString("o"), this.UserName, text, true);
        try { await Client.SendMessageAsync(message); txtMessInput.Clear(); } catch (Exception ex) { MessageBox.Show($"Gửi thất bại: {ex.Message}"); }
    }

    private async void btnCreate_Click(object sender, EventArgs e)
    {
        using (var dlg = new FormCreate())
        {
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                var roomName = dlg.RoomName;
                var pass = dlg.RoomPassword;
                if (string.IsNullOrEmpty(roomName)) return;
                await Client.SendMessageAsync(new CreateRoomMessage { Room = roomName, Password = pass });
            }
        }
    }

    private async void btnJoin_Click(object sender, EventArgs e)
    {
        if (lboxRooms.SelectedItem == null) { MessageBox.Show("Chọn phòng!"); return; }
        var roomName = lboxRooms.SelectedItem.ToString();

        using (var dlg = new FormJoinPassword(roomName))
        {
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                await Client.SendMessageAsync(new JoinRoomMessage { Room = roomName, Password = dlg.Password });
                UpdateChatContext(ChatContext.Room, roomName);
            }
        }
    }

    private void lboxUsers_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lboxUsers.SelectedItem == null) return;
        UpdateChatContext(ChatContext.Private, lboxUsers.SelectedItem.ToString());
    }

    private async void btnLeave_Click(object sender, EventArgs e)
    {
        if (_currentContext != ChatContext.Room) return;
        await Client.SendMessageAsync(new LeaveRoomMessage { Room = _currentContextTarget });
        UpdateChatContext(ChatContext.Public, "public");
    }

    private async void btnLogOut_Click(object sender, EventArgs e)
    {
        var confirm = MessageBox.Show("Đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo);
        if (confirm != DialogResult.Yes) return;
        try { await Client.SendMessageAsync(new LogoutMessage()); } catch { }
        Client.Disconnect(); _cts?.Cancel(); this.Close();
    }

    private void UpdateChatContext(ChatContext context, string target)
    {
        if (!string.IsNullOrEmpty(_currentContextTarget)) _chatHistories[_currentContextTarget] = rtbMessList.Rtf;
        _currentContext = context; _currentContextTarget = target;
        if (_chatHistories.ContainsKey(target)) rtbMessList.Rtf = _chatHistories[target]; else rtbMessList.Clear();

        switch (context)
        {
            case ChatContext.Public: lblNameRoom.Text = "Chat Công Khai"; btnLeave.Enabled = false; break;
            case ChatContext.Private: lblNameRoom.Text = $"Chat riêng: {target}"; btnLeave.Enabled = false; break;
            case ChatContext.Room: lblNameRoom.Text = $"Phòng: {target}"; btnLeave.Enabled = true; break;
        }
        rtbMessList.ScrollToCaret();
    }

    private void AppendChatMessage(string timeStr, string sender, string message, bool isSelf, Color? senderColor = null)
    {
        string time = DateTime.TryParse(timeStr, out var dt) ? dt.ToLocalTime().ToString("HH:mm:ss") : DateTime.Now.ToString("HH:mm:ss");
        rtbMessList.SelectionStart = rtbMessList.TextLength; rtbMessList.ScrollToCaret();
        rtbMessList.SelectionColor = Color.Gray; rtbMessList.AppendText($"{time} ");
        rtbMessList.SelectionColor = senderColor ?? (isSelf ? Color.Blue : Color.Purple); rtbMessList.AppendText($"{sender}: ");
        rtbMessList.SelectionColor = Color.Black; rtbMessList.AppendText($"{message}{Environment.NewLine}");
        if (!string.IsNullOrEmpty(_currentContextTarget)) _chatHistories[_currentContextTarget] = rtbMessList.Rtf;
    }

    // Các hàm UI trống (giữ nguyên để tránh lỗi Designer)
    private void Chat_TCP_Client_FormClosing(object s, FormClosingEventArgs e) { if (Client?.Username != null) { if (MessageBox.Show("Thoát?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.No) e.Cancel = true; else { Client.Disconnect(); _cts?.Cancel(); } } }
    private void lboxRooms_SelectedIndexChanged(object s, EventArgs e) { }
    private void rtbMessList_TextChanged(object s, EventArgs e) { }
    private void panel1_Paint(object s, PaintEventArgs e) { }
    private void Connect_Click(object s, EventArgs e) { }
    private void label1_Click(object s, EventArgs e) { }
    private void txtMessageInput_TextChanged(object s, EventArgs e) { }
    private void pnlMessageList_Paint(object s, PaintEventArgs e) { }
    private void pnlChatFame_Paint(object s, PaintEventArgs e) { }
    private void pnlChatHeader_Paint(object s, PaintEventArgs e) { }
}