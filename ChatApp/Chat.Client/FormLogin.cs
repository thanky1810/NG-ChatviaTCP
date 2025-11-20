using Chat.Shared;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat.Client
{
    public partial class FormLogin : Form
    {
        private ChatClient _chatClient;
        private CancellationTokenSource? _cts;
        private TaskCompletionSource<BaseMessage>? _loginResponseTcs;

        // ✅ Property 1: Để Program.cs lấy Client đã kết nối
        public ChatClient ConnectedClient => _chatClient;

        // ✅ Property 2: Để Program.cs lấy danh sách User/Room
        public LoginOkMessage? LoginOkDetails { get; private set; }

        public FormLogin()
        {
            InitializeComponent();
            this.txtHost.Text = "127.0.0.1";
            this.txtPort.Text = "8888";
            this.txtUserName.Text = "User_Client2";

            _chatClient = new ChatClient();
            this.btnConnect.Click += BtnConnect_Click;

            _chatClient.ConnectionStatusChanged += OnConnectionStatusChanged;
            _chatClient.MessageReceived += OnMessageReceived;
        }

        private void OnConnectionStatusChanged(string status)
        {
            if (!this.IsDisposed && this.IsHandleCreated)
                this.BeginInvoke((Action)(() => lblHost1.Text = status));
        }

        private async void BtnConnect_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text) || string.IsNullOrWhiteSpace(txtPort.Text) || string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnConnect.Enabled = false;
            _cts = new CancellationTokenSource();
            _loginResponseTcs = new TaskCompletionSource<BaseMessage>();

            try
            {
                await _chatClient.ConnectAsync(txtHost.Text.Trim(), int.Parse(txtPort.Text.Trim()), txtUserName.Text.Trim());

                if (await Task.WhenAny(_loginResponseTcs.Task, Task.Delay(5000, _cts.Token)) != _loginResponseTcs.Task)
                    throw new TimeoutException("Server không phản hồi.");

                var response = await _loginResponseTcs.Task;
                if (response is LoginOkMessage) { this.DialogResult = DialogResult.OK; this.Close(); }
                else if (response is ErrorMessage err) throw new Exception(err.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _chatClient.Disconnect(); _cts?.Cancel();
            }
            finally { btnConnect.Enabled = true; }
        }

        private void OnMessageReceived(BaseMessage message)
        {
            if (message is LoginOkMessage lok) { this.LoginOkDetails = lok; _loginResponseTcs?.TrySetResult(lok); }
            else if (message is ErrorMessage err) { _loginResponseTcs?.TrySetResult(err); }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_chatClient != null) { _chatClient.ConnectionStatusChanged -= OnConnectionStatusChanged; _chatClient.MessageReceived -= OnMessageReceived; }
            _cts?.Cancel();
            base.OnFormClosed(e);
        }

        // ✅ CÁC THUỘC TÍNH CÔNG KHAI MÀ PROGRAM.CS CẦN (Bạn đang thiếu phần này)
        public string Host => txtHost.Text.Trim();
        public int Port => int.TryParse(txtPort.Text.Trim(), out int p) ? p : 0;

        // 👇 Đây là cái Program.cs đang tìm kiếm
        public string UserName => txtUserName.Text.Trim();

        // Các hàm UI phụ trợ
        private void FormLogin_Load(object sender, EventArgs e) { }
        private void FormLogin_Shown(object sender, EventArgs e) { this.BeginInvoke((Action)(() => { this.ActiveControl = txtHost; txtHost.Focus(); })); }
        private void TxtHost_KeyDown(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; txtPort.Focus(); } }
        private void TxtPort_KeyDown(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; txtUserName.Focus(); } }
        private void TxtUserName_KeyDown(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnConnect_Click(null, null); } }
        private void TxtPort_KeyPress(object s, KeyPressEventArgs e) { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; }
        private void TxtPort_TextChanged(object s, EventArgs e) { }
        private void textBox1_TextChanged(object s, EventArgs e) { }
        private void label2_Click(object s, EventArgs e) { }
        private void panelLogin_Paint(object s, PaintEventArgs e) { }
        private void txtHost_TextChanged(object s, EventArgs e) { }
        private void lblLogin_Click(object s, EventArgs e) { }
    }
}