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
            // Không cho nút Connect nhận focus bằng Tab
            this.btnConnect.TabStop = false;

            // Đặt lại TabIndex cho 3 ô để Tab chỉ quay vòng giữa chúng
            this.txtHost.TabIndex = 0;
            this.txtPort.TabIndex = 1;
            this.txtUserName.TabIndex = 2;

            // Đăng ký sự kiện Shown — chạy sau khi form đã được hiển thị
            this.Shown += FormLogin_Shown;

            // Đăng ký các sự kiện bàn phím và click cho điều khiển
            this.txtHost.KeyDown += TxtHost_KeyDown;
            this.txtPort.KeyDown += TxtPort_KeyDown;
            this.txtUserName.KeyDown += TxtUserName_KeyDown;
            this.btnConnect.Click += BtnConnect_Click;

            // Giới hạn độ dài và đăng ký sự kiện cho các TextBox
            this.txtHost.MaxLength = 15;
            this.txtUserName.MaxLength = 15;

            // Port: chỉ cho nhập 4 chữ số
            this.txtPort.MaxLength = 4;
            this.txtPort.KeyPress += TxtPort_KeyPress;
            this.txtPort.TextChanged += TxtPort_TextChanged;
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

        // Chặn nhập ký tự không phải số ở ô Port
        private void TxtPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Cho phép phím điều khiển (backspace, etc.) và các chữ số
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // Xử lý khi paste hoặc thay đổi text: chỉ giữ các chữ số và cắt tối đa 4 ký tự
        private void TxtPort_TextChanged(object sender, EventArgs e)
        {
            var txt = txtPort.Text;
            if (string.IsNullOrEmpty(txt))
                return;

            // Lọc chỉ giữ chữ số
            var digitsOnlyChars = new System.Text.StringBuilder(txt.Length);
            foreach (char c in txt)
            {
                if (char.IsDigit(c))
                    digitsOnlyChars.Append(c);
            }

            var digitsOnly = digitsOnlyChars.ToString();

            // Cắt tối đa 4 chữ số
            if (digitsOnly.Length > 4)
                digitsOnly = digitsOnly.Substring(0, 4);

            if (digitsOnly != txt)
            {
                int selStart = txtPort.SelectionStart;
                txtPort.Text = digitsOnly;
                // Điều chỉnh con trỏ
                txtPort.SelectionStart = Math.Min(selStart, txtPort.Text.Length);
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

        private void panelLogin_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtHost_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
