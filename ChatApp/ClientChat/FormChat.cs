using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClientChat
{
    public partial class Chat_TCP_Client : Form
    {
        // Thuộc tính để nhận dữ liệu từ FormLogin
        public string UserName { get; set; }

        // Lưu trữ tin nhắn theo phòng: tên phòng -> danh sách tin nhắn
        private class RoomMessage
        {
            public string Time { get; set; }
            public string Sender { get; set; }
            public string Message { get; set; }
            public bool IsSelf { get; set; }
        }

        private readonly Dictionary<string, List<RoomMessage>> _roomMessages = new Dictionary<string, List<RoomMessage>>(StringComparer.OrdinalIgnoreCase);

        public Chat_TCP_Client()
        {
            InitializeComponent();
            this.Load += Chat_TCP_Client_Load;
        }

        private void Chat_TCP_Client_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                this.Text = $"Chat - {UserName}";

                // Thêm username vào ListBox users khi mở form (tránh trùng lặp)
                if (!lboxUsers.Items.Contains(UserName))
                {
                    lboxUsers.Items.Add(UserName);
                    // Tùy chọn: chọn user vừa thêm
                    lboxUsers.SelectedIndex = lboxUsers.Items.IndexOf(UserName);
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e) { }

        private void Connect_Click(object sender, EventArgs e) { }

        private void label1_Click(object sender, EventArgs e) { }

        private void btnLeave_Click(object sender, EventArgs e)
        {
            // Nếu không có phòng đang chọn / hiển thị, báo cho người dùng
            var currentRoom = lblNameRoom?.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(currentRoom) || currentRoom == "NameRoom")
            {
                MessageBox.Show("Bạn hiện không ở trong phòng nào.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Xác nhận rời phòng
            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn rời phòng '{currentRoom}'?",
                "Xác nhận rời phòng",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            // --- Hành động rời phòng: reset UI về trạng thái ban đầu của FormChat ---
            // Xóa giao diện hiển thị đoạn chat và input (nhưng KHÔNG xóa dữ liệu đã lưu)
            rtbMessList.Clear();
            txtMessInput.Clear();

            // Bỏ chọn phòng trong list và không hiện tên phòng
            try
            {
                lboxRooms.ClearSelected();
            }
            catch
            {
                // nếu null hoặc chưa khởi tạo thì bỏ qua
            }

            lblNameRoom.Text = "NameRoom";

            // Tùy chọn: chuyển focus về panel sidebar/rooms để người dùng chọn phòng khác
            if (lboxRooms.Items.Count > 0)
                lboxRooms.Focus();
            else
                this.ActiveControl = lboxUsers;

            // Thông báo đã rời phòng
            MessageBox.Show("Bạn đã rời phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Nếu sau này có kết nối server, đây là nơi nên gửi yêu cầu rời phòng lên server.
        }

        private void txtMessageInput_TextChanged(object sender, EventArgs e) { }

        private void pnlMessageList_Paint(object sender, PaintEventArgs e) { }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessInput.Text))
                return;

            string senderName = string.IsNullOrWhiteSpace(this.UserName) ? "Linh" : this.UserName;
            string time = DateTime.Now.ToString("HH:mm:ss");
            string message = txtMessInput.Text.Trim();

            // Lấy tên phòng hiện tại
            var currentRoom = lblNameRoom?.Text ?? string.Empty;

            // Hiển thị tin nhắn lên UI (nếu đang ở phòng)
            AppendChatMessage(time, senderName, message, true); // true = tin của mình

            // Nếu đang trong một phòng cụ thể thì lưu tin nhắn vào bộ nhớ theo phòng
            if (!string.IsNullOrWhiteSpace(currentRoom) && currentRoom != "NameRoom")
            {
                List<RoomMessage> list;
                if (!_roomMessages.TryGetValue(currentRoom, out list))
                {
                    list = new List<RoomMessage>();
                    _roomMessages[currentRoom] = list;
                }

                list.Add(new RoomMessage
                {
                    Time = time,
                    Sender = senderName,
                    Message = message,
                    IsSelf = true
                });
            }

            txtMessInput.Clear();
        }

        private void pnlChatFame_Paint(object sender, PaintEventArgs e) { }

        private void pnlChatHeader_Paint(object sender, PaintEventArgs e) { }

        private void lboxUsers_SelectedIndexChanged(object sender, EventArgs e) { }

        private void Chat_TCP_Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to close the chat?",
                "Confirm Leave",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            e.Cancel = (result != DialogResult.Yes);
        }

        // Tạo phòng: thêm tên phòng vào listboxRooms và chọn phòng đó
        private void btnCreate_Click(object sender, EventArgs e)
        {
            using (var dlg = new FormCreate())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var roomName = dlg.RoomName; // <-- Đọc từ property

                    if (!string.IsNullOrEmpty(roomName))
                    {
                        // Nếu chưa có phòng với tên đó thì thêm và chọn nó
                        if (!lboxRooms.Items.Contains(roomName))
                        {
                            lboxRooms.Items.Add(roomName);
                        }

                        // Tạo entry lưu trữ tin nhắn cho phòng mới (nếu chưa có)
                        if (!_roomMessages.ContainsKey(roomName))
                            _roomMessages[roomName] = new List<RoomMessage>();

                        // Chọn phòng mới thêm để người dùng có thể nhấn Join ngay
                        lboxRooms.SelectedIndex = lboxRooms.Items.IndexOf(roomName);
                    }
                }
            }
        }

        // Join phòng: vào phòng được chọn, hiển thị tên phòng trên lbNameRoom và load lại chat từ bộ nhớ
        private void btnJoin_Click(object sender, EventArgs e)
        {
            if (lboxRooms.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một phòng trong danh sách để Join.", "Chưa chọn phòng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var roomName = lboxRooms.SelectedItem.ToString();
            if (string.IsNullOrWhiteSpace(roomName))
            {
                MessageBox.Show("Tên phòng không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Hiển thị tên phòng hiện tại
            lblNameRoom.Text = roomName;

            // Reset giao diện chat trước khi load lại tin nhắn của phòng này
            rtbMessList.Clear();
            txtMessInput.Clear();

            // Nếu đã có tin nhắn lưu cho phòng này thì hiện lại chúng
            if (_roomMessages.TryGetValue(roomName, out var saved))
            {
                foreach (var m in saved)
                {
                    AppendChatMessage(m.Time, m.Sender, m.Message, m.IsSelf);
                }
            }

            // Chọn lại phòng trong listbox (đảm bảo highlight)
            lboxRooms.SelectedIndex = lboxRooms.Items.IndexOf(roomName);

            // Đặt focus vào ô nhập tin nhắn
            txtMessInput.Focus();

            // Thông báo đã vào phòng
            MessageBox.Show($"Bạn đã vào phòng '{roomName}'.", "Đã vào phòng", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Nếu có server, gửi yêu cầu join phòng ở đây.
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            // Xác nhận đăng xuất
            var confirm = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            // Xóa toàn bộ đoạn chat hiển thị và input trước khi ẩn form/đăng xuất
            rtbMessList.Clear();
            txtMessInput.Clear();

            // Ẩn form hiện tại trong khi hiển thị dialog Login
            this.Hide();

            using (var login = new FormLogin())
            {
                // Hiển thị modal để nhập lại thông tin (nếu người dùng muốn đăng nhập lại)
                if (login.ShowDialog() == DialogResult.OK)
                {
                    // Lưu tên cũ để có thể xoá khỏi list nếu cần
                    var oldUser = this.UserName;

                    // Cập nhật username mới từ dialog
                    this.UserName = login.UserName;
                    this.Text = $"Chat - {this.UserName}";

                    // Cập nhật ListBox users: remove old, add new (tránh trùng)
                    if (!string.IsNullOrWhiteSpace(oldUser))
                        lboxUsers.Items.Remove(oldUser);

                    if (!string.IsNullOrWhiteSpace(this.UserName) && !lboxUsers.Items.Contains(this.UserName))
                        lboxUsers.Items.Add(this.UserName);

                    lboxUsers.SelectedIndex = lboxUsers.Items.IndexOf(this.UserName);

                    // Hiển thị lại form chat
                    this.Show();
                }
                else
                {
                    // Nếu người dùng huỷ (không muốn đăng nhập lại), đóng ứng dụng / form hiện tại
                    this.Close();
                }
            }
        }

        private void lboxRooms_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void rtbMessList_TextChanged(object sender, EventArgs e)
        {

        }
        private void AppendChatMessage(string time, string sender, string message, bool isSelf)
        {
            rtbMessList.SelectionStart = rtbMessList.TextLength;

            // Phần thời gian
            rtbMessList.SelectionColor = Color.Gray;
            rtbMessList.AppendText($"{time} ");

            // Phần tên
            rtbMessList.SelectionColor = isSelf ? Color.Blue : Color.Purple; // xanh cho mình, tím cho người khác
            rtbMessList.AppendText($"{sender}: ");

            // Phần nội dung
            rtbMessList.SelectionColor = Color.Black;
            rtbMessList.AppendText($"{message}{Environment.NewLine}");

            // 👉 Reset màu về đen cho chắc chắn dòng sau không bị lem
            rtbMessList.SelectionColor = Color.Black;

            // Cuộn xuống cuối
            rtbMessList.SelectionStart = rtbMessList.TextLength;
            rtbMessList.ScrollToCaret();
        }

    }
}
