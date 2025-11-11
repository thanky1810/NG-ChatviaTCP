// File: UI.Chat/FormChat.cs
// (Người 4 - Nguyễn Thị Hoài Linh: Logic Màn hình Chat chính)
// (Người 6 - Cao Xuân Quyết: Tích hợp Gửi tin, Rooms, Logout)
using Chat.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ClientChat
{
    public partial class Chat_TCP_Client : Form
    {
        // (Người 5) Nhận Lõi Client từ Program.cs
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChatClient Client { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UserName { get; set; }

        // (Người 6) Nhận danh sách User/Room ban đầu từ FormLogin
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LoginOkMessage? InitialLoginOk { get; set; }

        private CancellationTokenSource _cts;
        private enum ChatContext { Public, Private, Room }
        private ChatContext _currentContext = ChatContext.Public;
        private string _currentContextTarget = "public";

        // (Người 4) Bộ nhớ đệm (Dictionary) để lưu lịch sử chat
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

            // (Người 4 & 5) Đăng ký nhận message từ Lõi Client (Người 5)
            Client.MessageReceived += ProcessMessage;

            // (Người 4) Khởi tạo context đầu tiên là "Chat Công Khai"
            UpdateChatContext(ChatContext.Public, "public");

            // (Người 4 & 6) Hiển thị danh sách User/Room ban đầu
            if (InitialLoginOk != null)
            {
                ProcessUserList(InitialLoginOk.Users);
                ProcessRoomList(InitialLoginOk.Rooms);
            }
        }

        // (Người 4) Hàm helper cập nhật UI danh sách User
        private void ProcessUserList(List<string> users)
        {
            lboxUsers.Items.Clear();
            foreach (var user in users)
            {
                if (user != this.UserName) lboxUsers.Items.Add(user);
            }
        }

        // (Người 4) Hàm helper cập nhật UI danh sách Phòng
        private void ProcessRoomList(List<string> rooms)
        {
            lboxRooms.Items.Clear();
            foreach (var room in rooms)
                lboxRooms.Items.Add(room);
        }

        // (Người 4 & 5) Hàm xử lý message nhận được từ Lõi Client (Người 5)
        private void ProcessMessage(BaseMessage message)
        {
            // (Người 5) Đảm bảo an toàn luồng khi cập nhật UI
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)(() => ProcessMessage(message)));
                return;
            }

            string messageContext = "";
            string sender = "??";
            string text = "";
            string ts = DateTime.UtcNow.ToString("o");

            // (Người 4) Phân loại tin nhắn
            switch (message)
            {
                case ChatPublicMessage chat:
                    if (chat.From == this.UserName) return; // Bỏ qua tin của chính mình
                    messageContext = "public";
                    sender = chat.From; text = chat.Text; ts = chat.Timestamp;
                    break;
                case ChatPrivateMessage dm:
                    if (dm.From == this.UserName) return; // Bỏ qua tin của chính mình
                    messageContext = dm.From; // Context là người gửi (mình là người nhận)
                    sender = dm.From; text = dm.Text; ts = dm.Timestamp;
                    break;
                case ChatRoomMessage roomChat:
                    if (roomChat.From == this.UserName) return; // Bỏ qua tin của chính mình
                    messageContext = roomChat.Room;
                    sender = roomChat.From; text = roomChat.Text; ts = roomChat.Timestamp;
                    break;

                // (Người 4 & 6) Xử lý tin hệ thống/lỗi (hiển thị ở tab hiện tại)
                case SystemMessage sys:
                    AppendChatMessage(ts, "Hệ thống", sys.Text, false, Color.DarkGoldenrod);
                    return;
                case ErrorMessage err:
                    AppendChatMessage(ts, "Lỗi", err.Message, false, Color.Red);
                    return;

                // (Người 4) Xử lý cập nhật danh sách
                case UserListMessage userList:
                    ProcessUserList(userList.Users);
                    return;
                case RoomListMessage roomList:
                    ProcessRoomList(roomList.Rooms);
                    return;
            }

            // (Người 4) Hiển thị hoặc lưu tin nhắn
            if (messageContext == _currentContextTarget)
            {
                // Nếu tin nhắn thuộc tab đang mở, hiển thị ngay
                AppendChatMessage(ts, sender, text, false);
            }
            else if (!string.IsNullOrEmpty(messageContext))
            {
                // (Người 4) Cải tiến: Nếu tab không mở, lưu vào lịch sử (dictionary)
                string oldRtf = _chatHistories.ContainsKey(messageContext) ? _chatHistories[messageContext] : "";
                using (var tempRtb = new RichTextBox())
                {
                    if (!string.IsNullOrEmpty(oldRtf)) tempRtb.Rtf = oldRtf;

                    string time = DateTime.TryParse(ts, out var dt) ? dt.ToLocalTime().ToString("HH:mm:ss") : DateTime.Now.ToString("HH:mm:ss");
                    tempRtb.SelectionStart = tempRtb.TextLength;
                    tempRtb.SelectionColor = Color.Gray;
                    tempRtb.AppendText($"{time} ");
                    tempRtb.SelectionColor = Color.Purple; // Tin nhắn nhận được luôn là màu tím
                    tempRtb.AppendText($"{sender}: ");
                    tempRtb.SelectionColor = Color.Black;
                    tempRtb.AppendText($"{text}{Environment.NewLine}");

                    _chatHistories[messageContext] = tempRtb.Rtf;
                }
            }
        }

        // (Người 6) Tích hợp Gửi tin (UC-02, UC-03, Chat Room)
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

            // (Người 4) Hiển thị tin nhắn của chính mình ngay lập tức (Optimistic Send)
            AppendChatMessage(DateTime.UtcNow.ToString("o"), this.UserName, text, true);

            try
            {
                // (Người 6 & 5) Gọi Lõi Client (Người 5) để gửi
                await Client.SendMessageAsync(message);
                txtMessInput.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gửi thất bại: {ex.Message}");
            }
        }

        // (Người 6) Tích hợp Tạo phòng (UC-04)
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

        // (Người 6) Tích hợp Vào phòng (UC-04)
        private async void btnJoin_Click(object sender, EventArgs e)
        {
            if (lboxRooms.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var roomName = lboxRooms.SelectedItem.ToString();
            await Client.SendMessageAsync(new JoinRoomMessage { Room = roomName });

            // (Người 4) Chuyển giao diện sang phòng vừa vào
            UpdateChatContext(ChatContext.Room, roomName);
        }

        // (Người 4) Xử lý Double-click user để mở Chat Riêng (UC-03)
        private void lboxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lboxUsers.SelectedItem == null) return;
            var username = lboxUsers.SelectedItem.ToString();
            UpdateChatContext(ChatContext.Private, username);
        }

        // (Người 6) Tích hợp Rời phòng (UC-04)
        private async void btnLeave_Click(object sender, EventArgs e)
        {
            if (_currentContext != ChatContext.Room)
            {
                MessageBox.Show("Bạn không ở trong phòng nào.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await Client.SendMessageAsync(new LeaveRoomMessage { Room = _currentContextTarget });

            // (Người 4 & 6) Chuyển UI về "Chat Công Khai"
            UpdateChatContext(ChatContext.Public, "public");
        }

        // (Người 6) Tích hợp Logout (UC-06)
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

            // Đóng FormChat, Program.cs (vòng lặp while) sẽ mở lại FormLogin
            this.Close();
        }

        // (Người 4) Cải tiến: Hàm này LƯU và TẢI lịch sử chat
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
                    btnLeave.Enabled = false; // Không thể rời Public
                    break;
                case ChatContext.Private:
                    lblNameRoom.Text = $"Chat riêng: {target}";
                    btnLeave.Enabled = false; // Không thể rời Chat riêng
                    break;
                case ChatContext.Room:
                    lblNameRoom.Text = $"Phòng: {target}";
                    btnLeave.Enabled = true; // Có thể rời phòng
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

            // (Người 4) Tin nhắn của mình (isSelf) màu xanh, người khác màu tím
            rtbMessList.SelectionColor = senderColor ?? (isSelf ? Color.Blue : Color.Purple);
            rtbMessList.AppendText($"{sender}: ");

            rtbMessList.SelectionColor = Color.Black;
            rtbMessList.AppendText($"{message}{Environment.NewLine}");

            // (Người 4) Cập nhật lịch sử (Dictionary)
            if (!string.IsNullOrEmpty(_currentContextTarget))
            {
                _chatHistories[_currentContextTarget] = rtbMessList.Rtf;
            }
        }

        #region (Người 4 & 6 - Các hàm UI phụ trợ)
        private void Chat_TCP_Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Xử lý khi người dùng nhấn nút X (thoát)
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
}