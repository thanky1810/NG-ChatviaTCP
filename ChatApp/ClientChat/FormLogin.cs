using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClientChat
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
            // Đăng ký sự kiện Shown — chạy sau khi form đã được hiển thị
            this.Shown += FormLogin_Shown;

            // Đăng ký các sự kiện bàn phím và click cho điều khiển
            this.txtHost.KeyDown += TxtHost_KeyDown;
            this.txtPort.KeyDown += TxtPort_KeyDown;
            this.txtUserName.KeyDown += TxtUserName_KeyDown;
            this.btnConnect.Click += BtnConnect_Click;
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            // (Giữ hoặc bỏ tùy bạn) Load có thể bị ghi đè, nên dùng Shown
        }

        private void FormLogin_Shown(object sender, EventArgs e)
        {
            // Đảm bảo focus được đặt sau khi form hiển thị
            this.BeginInvoke((Action)(() =>
            {
                // Đặt active control và chọn toàn bộ text để gõ đè dễ dàng
                this.ActiveControl = txtHost;
                txtHost.Focus();
                txtHost.SelectAll();
            }));
        }

        private void TxtHost_KeyDown(object sender, KeyEventArgs e)
        {
            // Nhấn Enter hoặc mũi tên xuống -> xuống ô Port
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = true;
                txtPort.Focus();
                txtPort.SelectAll();
            }
            // (tùy chọn) mũi tên lên ở Host không làm gì
            else if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void TxtPort_KeyDown(object sender, KeyEventArgs e)
        {
            // Nhấn Enter hoặc mũi tên xuống -> xuống ô UserName
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = true;
                txtUserName.Focus();
                txtUserName.SelectAll();
            }
            // Nhấn mũi tên lên -> lên ô Host
            else if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
                txtHost.Focus();
                txtHost.SelectAll();
            }
        }

        private void TxtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            // Nhấn mũi tên lên -> lên ô Port
            if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
                txtPort.Focus();
                txtPort.SelectAll();
            }
            // Nhấn Enter trên UserName -> kích hoạt nút Connect
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                // Thực hiện hành động tương tự khi nhấn chuột vào CONNECT
                BtnConnect_Click(this.btnConnect, EventArgs.Empty);
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            // Kiểm tra nhanh: đảm bảo các trường không để trống
            if (string.IsNullOrWhiteSpace(txtHost.Text) ||
                string.IsNullOrWhiteSpace(txtPort.Text) ||
                string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show("Vui lòng nhập Host, Port và User Name.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Nếu cần, có thể parse port ở đây:
            // int port;
            // if (!int.TryParse(txtPort.Text.Trim(), out port)) { ... }

            // Trả DialogResult.OK để Program biết mở form chat
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        // Các thuộc tính để Program hoặc form khác đọc sau khi dialog đóng
        public string Host => txtHost.Text.Trim();

        public int Port
        {
            get
            {
                if (int.TryParse(txtPort.Text.Trim(), out int p))
                    return p;
                return 0;
            }
        }

        public string UserName => txtUserName.Text.Trim();
    }
}
