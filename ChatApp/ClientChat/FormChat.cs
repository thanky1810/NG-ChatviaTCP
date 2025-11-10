// File: UI.Chat/FormChat.cs (ĐÃ SỬA LỖI "LEAVE ROOM")
using Chat.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

// Đảm bảo namespace này khớp với project của bạn
namespace ClientChat // (Hoặc namespace Chat.Client)
{
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

        // ✅ CẢI TIẾN: Bộ nhớ đệm lưu lịch sử chat
        private Dictionary<string, string> _chatHistories = new Dictionary<string, string>();

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

            UpdateChatContext(ChatContext.Public, "public"); // Khởi tạo context đầu tiên

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
                    messageContext = "public";
                    sender = chat.From; text = chat.Text; ts = chat.Timestamp;
                    break;
                case ChatPrivateMessage dm:
                    messageContext = (dm.From == this.UserName) ? dm.To : dm.From;
                    sender = dm.From; text = dm.Text; ts = dm.Timestamp;
                    break;
                case ChatRoomMessage roomChat:
                    messageContext = roomChat.Room;
                    sender = roomChat.From; text = roomChat.Text; ts = roomChat.Timestamp;
                    break;

                // Tin hệ thống/lỗi thì hiển thị ở tab hiện tại
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

            // Nếu tin nhắn thuộc tab đang mở, hiển thị ngay
            if (messageContext == _currentContextTarget)
            {
                AppendChatMessage(ts, sender, text, sender == this.UserName);
            }
            else
            {
                // (Nâng cao) Báo tin nhắn mới
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string text = txtMessInput.Text;
            if (string.IsNullOrWhiteSpace(text) || Client == null) return;

            BaseMessage message;
            switch (_currentContext)
            {
                case ChatContext.Public:
                    message = new ChatPublicMessage { Text = text };
                    break;
                case ChatContext.Private:
                    message = new ChatPrivateMessage { To = _currentContextTarget, Text = text };
                    break;
                case ChatContext.Room:
                    message = new ChatRoomMessage { Room = _currentContextTarget, Text = text };
                    break;
                default:
                    return;
            }

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

        private async void btnCreate_Click(object sender, EventArgs e)
        {
            using (var dlg = new FormCreate())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var roomName = dlg.RoomName;
                    if (string.IsNullOrEmpty(roomName)) return;
                    await Client.SendMessageAsync(new CreateRoomMessage { Room = roomName });
                }
            }
        }

        private async void btnJoin_Click(object sender, EventArgs e)
        {
            if (lboxRooms.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var roomName = lboxRooms.SelectedItem.ToString();
            await Client.SendMessageAsync(new JoinRoomMessage { Room = roomName });
            UpdateChatContext(ChatContext.Room, roomName);
        }

        private void lboxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lboxUsers.SelectedItem == null) return;
            var username = lboxUsers.SelectedItem.ToString();
            UpdateChatContext(ChatContext.Private, username);
        }

        //
        // ✅ ĐÂY LÀ SỬA LỖI CỦA BẠN
        //
        private async void btnLeave_Click(object sender, EventArgs e)
        {
            if (_currentContext != ChatContext.Room)
            {
                // Nút này chỉ nên được bật khi ở trong phòng,
                // nhưng chúng ta vẫn kiểm tra cho chắc
                MessageBox.Show("Bạn không ở trong phòng nào.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 1. Gửi yêu cầu rời phòng
            await Client.SendMessageAsync(new LeaveRoomMessage { Room = _currentContextTarget });

            // 2. ✅ SỬA LỖI: Ngay lập tức chuyển UI về "Chat Công Khai"
            UpdateChatContext(ChatContext.Public, "public");
        }

        private async void btnLogOut_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                await Client.SendMessageAsync(new LogoutMessage());
            }
            catch { }

            Client.Disconnect();
            _cts.Cancel();

            this.Close(); // Program.cs sẽ mở lại FormLogin
        }

        // ✅ CẢI TIẾN: Hàm này giờ đây sẽ LƯU và TẢI lịch sử chat
        private void UpdateChatContext(ChatContext context, string target)
        {
            // 1. LƯU LỊCH SỬ CŨ (dùng .Rtf để giữ màu)
            if (!string.IsNullOrEmpty(_currentContextTarget))
            {
                _chatHistories[_currentContextTarget] = rtbMessList.Rtf;
            }

            _currentContext = context;
            _currentContextTarget = target;

            // 2. TẢI LỊCH SỬ MỚI
            if (_chatHistories.ContainsKey(target))
            {
                rtbMessList.Rtf = _chatHistories[target];
            }
            else
            {
                rtbMessList.Clear();
            }

            // 3. Cập nhật tiêu đề và nút
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

        private void AppendChatMessage(string timeStr, string sender, string message, bool isSelf, Color? senderColor = null)
        {
            string time = DateTime.TryParse(timeStr, out var dt)
                ? dt.ToLocalTime().ToString("HH:mm:ss")
                : DateTime.Now.ToString("HH:mm:ss");

            rtbMessList.SelectionStart = rtbMessList.TextLength;
            rtbMessList.SelectionColor = Color.Gray;
            rtbMessList.AppendText($"{time} ");

            rtbMessList.SelectionColor = senderColor ?? (isSelf ? Color.Blue : Color.Purple);
            rtbMessList.AppendText($"{sender}: ");

            rtbMessList.SelectionColor = Color.Black;
            rtbMessList.AppendText($"{message}{Environment.NewLine}");

            rtbMessList.SelectionStart = rtbMessList.TextLength;
            rtbMessList.ScrollToCaret();
        }

        #region (Các hàm UI gốc)
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
        private void pnlChatHeader_Paint(object sender, PaintEventArgs e) { }
        #endregion
    }
}