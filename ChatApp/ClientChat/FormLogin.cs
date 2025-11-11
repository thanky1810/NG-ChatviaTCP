// File: UI.Chat/FormLogin.cs
// (Người 6 - Cao Xuân Quyết: Logic Màn hình Login & Tích hợp UC-01)
using Chat.Shared;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientChat
{
    public partial class FormLogin : Form
    {
        // (Người 6 & 5) Tích hợp Lõi Client (Người 5)
        private ChatClient _chatClient;
        private CancellationTokenSource? _cts;
        private TaskCompletionSource<BaseMessage>? _loginResponseTcs;

        // Property để Program.cs lấy Client đã kết nối
        public ChatClient ConnectedClient => _chatClient;

        // Property để Program.cs lấy danh sách user/room ban đầu
        public LoginOkMessage? LoginOkDetails { get; private set; }

        public FormLogin()
        {
            InitializeComponent();

            // (Người 6) Cài đặt giá trị mặc định để test
            this.txtHost.Text = "127.0.0.1";
            this.txtPort.Text = "8888";
            this.txtUserName.Text = "UserWinForms"; // Đổi tên khi chạy client 2

            // (Người 6 & 5) Khởi tạo và đăng ký sự kiện từ Lõi Client (Người 5)
            _chatClient = new ChatClient();
            this.btnConnect.Click += BtnConnect_Click;
            _chatClient.ConnectionStatusChanged += (status) => this.BeginInvoke((Action)(() => lblHost1.Text = status));
            _chatClient.MessageReceived += OnMessageReceived;

            // (Người 6) Cài đặt UI (Validation, TabIndex...)
            this.btnConnect.TabStop = false;
            this.txtHost.TabIndex = 0;
            this.txtPort.TabIndex = 1;
            this.txtUserName.TabIndex = 2;
            this.Shown += FormLogin_Shown;
            this.txtHost.KeyDown += TxtHost_KeyDown;
            this.txtPort.KeyDown += TxtPort_KeyDown;
            this.txtUserName.KeyDown += TxtUserName_KeyDown;
            this.txtHost.MaxLength = 15;
            this.txtUserName.MaxLength = 15;
            this.txtPort.MaxLength = 4;
            this.txtPort.KeyPress += TxtPort_KeyPress;
            this.txtPort.TextChanged += TxtPort_TextChanged;
        }

        // (Người 6) Xử lý nút Connect (Tích hợp UC-01)
        private async void BtnConnect_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text) ||
                string.IsNullOrWhiteSpace(txtPort.Text) ||
                string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show("Vui lòng nhập Host, Port và User Name.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnConnect.Enabled = false;
            _cts = new CancellationTokenSource();
            _loginResponseTcs = new TaskCompletionSource<BaseMessage>();

            try
            {
                // (Người 6 & 5) Gọi hàm Connect (Người 5)
                await _chatClient.ConnectAsync(
                    txtHost.Text.Trim(),
                    int.Parse(txtPort.Text.Trim()),
                    txtUserName.Text.Trim()
                );

                // (Người 6) Chờ phản hồi LoginOk hoặc Error từ Server
                var responseTask = _loginResponseTcs.Task;
                if (await Task.WhenAny(responseTask, Task.Delay(5000, _cts.Token)) != responseTask)
                {
                    throw new TimeoutException("Server không phản hồi.");
                }

                // (Người 6) Xử lý phản hồi
                var response = await responseTask;
                if (response is LoginOkMessage)
                {
                    // Đăng nhập thành công, đóng FormLogin
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else if (response is ErrorMessage err)
                {
                    throw new Exception(err.Message); // Ném lỗi (vd: trùng tên)
                }
                else
                {
                    throw new Exception("Phản hồi login không hợp lệ.");
                }
            }
            catch (Exception ex)
            {
                // (Người 6) Hiển thị lỗi và ngắt kết nối
                MessageBox.Show($"Đăng nhập thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _chatClient.Disconnect();
                _cts?.Cancel();
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        // (Người 6 & 5) Xử lý message nhận được từ Lõi Client (Người 5)
        private void OnMessageReceived(BaseMessage message)
        {
            // Chỉ xử lý các message liên quan đến Login
            if (message is LoginOkMessage lok)
            {
                this.LoginOkDetails = lok; // Lưu lại data cho FormChat
                _loginResponseTcs?.TrySetResult(lok);
            }
            else if (message is ErrorMessage err)
            {
                _loginResponseTcs?.TrySetResult(err);
            }
        }

        // (Người 6 & 5) Dọn dẹp
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _chatClient.MessageReceived -= OnMessageReceived;
            _cts?.Cancel();
            base.OnFormClosed(e);
        }

        public string Host => txtHost.Text.Trim();
        public int Port => int.TryParse(txtPort.Text.Trim(), out int p) ? p : 0;
        public string UserName => txtUserName.Text.Trim();

        #region (Người 6 - Các hàm UI phụ trợ)
        private void FormLogin_Load(object sender, EventArgs e) { }
        private void FormLogin_Shown(object sender, EventArgs e)
        {
            this.BeginInvoke((Action)(() => { this.ActiveControl = txtHost; txtHost.Focus(); txtHost.SelectAll(); }));
        }
        private void TxtHost_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down) { e.SuppressKeyPress = true; txtPort.Focus(); txtPort.SelectAll(); }
            else if (e.KeyCode == Keys.Up) { e.SuppressKeyPress = true; }
        }
        private void TxtPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down) { e.SuppressKeyPress = true; txtUserName.Focus(); txtUserName.SelectAll(); }
            else if (e.KeyCode == Keys.Up) { e.SuppressKeyPress = true; txtHost.Focus(); txtHost.SelectAll(); }
        }
        private void TxtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) { e.SuppressKeyPress = true; txtPort.Focus(); txtPort.SelectAll(); }
            else if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnConnect_Click(this.btnConnect, EventArgs.Empty); }
        }
        private void TxtPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) { e.Handled = true; }
        }
        private void TxtPort_TextChanged(object sender, EventArgs e)
        {
            var txt = txtPort.Text;
            if (string.IsNullOrEmpty(txt)) return;
            var digitsOnlyChars = new System.Text.StringBuilder(txt.Length);
            foreach (char c in txt) { if (char.IsDigit(c)) digitsOnlyChars.Append(c); }
            var digitsOnly = digitsOnlyChars.ToString();
            if (digitsOnly.Length > 4) digitsOnly = digitsOnly.Substring(0, 4);
            if (digitsOnly != txt) { int selStart = txtPort.SelectionStart; txtPort.Text = digitsOnly; txtPort.SelectionStart = Math.Min(selStart, txtPort.Text.Length); }
        }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void panelLogin_Paint(object sender, PaintEventArgs e) { }
        private void txtHost_TextChanged(object sender, EventArgs e) { }
        private void lblLogin_Click(object sender, EventArgs e) { }
        #endregion
    }
}