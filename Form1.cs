namespace HttpListening
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    internal partial class Form1 : Form
    {
        private readonly CheapHttpServer server;

        public Form1()
        {
            this.InitializeComponent();

            this.server = new CheapHttpServer();

            var addLog = new EventHandler((s, e) =>
            {
                var text = string.Format("{0} {1}", DateTime.Now, ((Control)s).Text);
                this.TextBox1.AppendText(text);
                this.TextBox1.AppendText(Environment.NewLine);
            });

            this.Button1.Click += (s, e) =>
            {
                this.server.Start();
            };

            this.Button2.Click += (s, e) =>
            {
                this.server.Stop();
            };

            this.Button3.Click += (s, e) =>
            {
                this.server.Close();
            };

            this.Button4.Click += (s, e) =>
            {
                var info = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "rundll32.exe",
                    Arguments = "url.dll,FileProtocolHandler http://localhost:8080/",
                    CreateNoWindow = false,
                };

                System.Diagnostics.Process.Start(info);
            };

            this.Controls.OfType<Button>()
                .ToList()
                .ForEach(x => x.Click += addLog);
        }

        public Button Button1 { get; private set; }

        public Button Button2 { get; private set; }

        public Button Button3 { get; private set; }

        public Button Button4 { get; private set; }

        public TextBox TextBox1 { get; private set; }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            this.server.Dispose();
        }

        protected override void OnShown(EventArgs e)
        {
        }
    }
}
