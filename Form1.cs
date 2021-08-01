namespace HttpListening
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    internal partial class Form1 : Form
    {
        private CheapHttpServer server;

        private State state;

        public Form1()
        {
            this.InitializeComponent();

            var addLog = new EventHandler((s, e) =>
            {
                var text = string.Format("{0} {1}", DateTime.Now, ((Control)s).Text);
                this.TextBox3.AppendText(text);
                this.TextBox3.AppendText(Environment.NewLine);
            });

            this.Controls.OfType<Button>()
                .Where(x => x.Tag == null)
                .ToList()
                .ForEach(x => x.Click += addLog);

            this.Controls.OfType<Button>()
                .ToList()
                .ForEach(x => x.TabStop = false);

            this.AcceptCommandLineArgs();
            this.ResetState();

            this.NotifyIcon1.Click += (s, e) =>
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                this.NotifyIcon1.Visible = false;
            };

            this.Resize += (s, e) =>
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.NotifyIcon1.Visible = true;
                    this.Visible = false;
                }
            };
        }

        private enum State
        {
            Closed,
            Started,
            Stopped,
        }

        public Button Button1 { get; private set; }

        public Button Button2 { get; private set; }

        public Button Button3 { get; private set; }

        public Button Button4 { get; private set; }

        public Button Button5 { get; private set; }

        public TextBox TextBox1 { get; private set; }

        public TextBox TextBox2 { get; private set; }

        public TextBox TextBox3 { get; private set; }

        public Label Label1 { get; private set; }

        public Label Label2 { get; private set; }

        public Label Label3 { get; private set; }

        public ComboBox ComboBox1 { get; private set; }

        public NotifyIcon NotifyIcon1 { get; private set; }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            this.CloseServer();
        }

        protected override void OnShown(EventArgs e)
        {
        }

        private void AcceptCommandLineArgs()
        {
            var commandLineArgs = Environment.GetCommandLineArgs().Skip(1);

            int i;
            string port = commandLineArgs.FirstOrDefault(x => int.TryParse(x, out i));
            if (string.IsNullOrEmpty(port))
            {
                port = "8080";
            }

            this.TextBox1.Text = System.Text.RegularExpressions.Regex.Replace(port, "[^0-9]", string.Empty);

            var contentRoot = commandLineArgs.FirstOrDefault(x => System.IO.Directory.Exists(x));
            if (string.IsNullOrEmpty(contentRoot))
            {
                contentRoot = Environment.CurrentDirectory;
            }

            this.TextBox2.Text = contentRoot;
        }

        private void AppendLog(object obj, string note)
        {
            var target = (obj as Control) != null ?
                (obj as Control).Text : string.Empty;
            var text = string.Format("{0} {1} {2}", DateTime.Now, target, note);
            this.TextBox3.AppendText(text);
            this.TextBox3.AppendText(Environment.NewLine);
        }

        private void CloseServer()
        {
            if (this.state == State.Closed)
            {
                return;
            }

            this.server.Dispose();
            this.SetState(State.Closed);
        }

        private void ResetState()
        {
            this.SetState(State.Closed);
        }

        private void SetState(State value)
        {
            this.TextBox1.ReadOnly = !(value == State.Closed);
            this.TextBox2.ReadOnly = !(value == State.Closed);
            this.Button1.Enabled = value == State.Closed;
            this.Button2.Enabled = value != State.Started;
            this.Button3.Enabled = value == State.Started;
            this.Button4.Enabled = value != State.Closed;
            this.Button5.Enabled = !(value == State.Closed);
            this.state = value;
        }

        private void StartServer()
        {
            if (this.state == State.Started)
            {
                return;
            }

            if (this.state == State.Closed)
            {
                var port = string.IsNullOrEmpty(this.TextBox1.Text) ?
                    0 : Convert.ToInt32(this.TextBox1.Text);
                var contentRoot = this.TextBox2.Text;
                var concurrent = Convert.ToInt32(this.ComboBox1.Text);
                try
                {
                    this.server = new CheapHttpServer(contentRoot, port)
                    {
                        ConcurrentRequests = concurrent,
                    };

                    this.TextBox1.Text = this.server.Port.ToString();
                }
                catch (Exception ex)
                {
                    this.TextBox3.AppendText(ex.Message);
                    this.TextBox3.AppendText(Environment.NewLine);
                    this.server = null;
                    return;
                }
            }

            this.server.Start();
            this.SetState(State.Started);
        }

        private void StopServer()
        {
            if (this.state == State.Started)
            {
                this.server.Stop();
                this.SetState(State.Stopped);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.TextBox2.Text = dialog.SelectedPath;
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.StartServer();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            this.StopServer();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            this.CloseServer();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            if (this.server == null)
            {
                return;
            }

            var port = this.server.Port;
            var info = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "rundll32.exe",
                Arguments = string.Format("url.dll,FileProtocolHandler http://localhost:{0}/", port),
                CreateNoWindow = false,
            };

            System.Diagnostics.Process.Start(info);
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar);
        }

        private void TextBox1_Leave(object sender, EventArgs e)
        {
            var text = (sender as TextBox).Text;

            if (System.Text.RegularExpressions.Regex.IsMatch(text, "[^0-9]"))
            {
                (sender as TextBox).Text = System.Text.RegularExpressions.Regex.Replace(text, "[^0-9]", string.Empty);
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.state != State.Closed)
            {
                var concurrent = Convert.ToInt32(this.ComboBox1.Text);
                this.server.ConcurrentRequests = concurrent;
            }
        }
    }
}
