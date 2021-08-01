namespace HttpListening
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    internal partial class Form1 : Form
    {
        private void InitializeComponent()
        {
            this.Label1 = new Label
            {
                Text = "http://localhost:{Port}/",
                Left = 8,
                Top = 8,
                Width = 130,
                AutoSize = true,
            };

            this.Label2 = new Label
            {
                Text = "ContentRoot",
                Left = this.Label1.Left,
                Top = this.Label1.Top + this.Label1.Height,
            };

            this.TextBox1 = new TextBox
            {
                Left = this.Label1.Left + this.Label1.Width,
                Top = this.Label1.Top,
                Width = 48,
            };

            this.TextBox1.KeyPress += this.TextBox1_KeyPress;
            this.TextBox1.Leave += this.TextBox1_Leave;

            this.Label3 = new Label
            {
                Text = "Concurrent",
                Left = this.TextBox1.Left + this.TextBox1.Width + 24,
                Top = this.Label1.Top,
                Width = 60,
                AutoSize = true,
            };

            this.TextBox2 = new TextBox
            {
                Left = this.Label1.Left,
                Top = this.Label1.Top + this.Label1.Height + 16,
                Width = 250,
            };

            this.TextBox3 = new TextBox
            {
                Left = this.Label1.Left,
                Top = this.TextBox2.Top + this.TextBox2.Height + 8,
                Width = 300,
                Height = 100,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
            };

            this.Button1 = new Button
            {
                Text = "browse",
                Left = this.TextBox2.Left + this.TextBox2.Width + 8,
                Top = this.TextBox2.Top,
                Width = 64,
                Tag = new object(),
            };

            this.Button1.Click += this.Button1_Click;

            this.Button2 = new Button
            {
                Text = "Server.Start()",
                Left = this.Label1.Left,
                Top = this.TextBox3.Top + this.TextBox3.Height + 8,
                Width = 100,
            };

            this.Button2.Click += this.Button2_Click;

            this.Button3 = new Button
            {
                Text = "Server.Stop()",
                Left = this.Button2.Left + this.Button2.Width + 8,
                Top = this.Button2.Top,
                Width = this.Button2.Width,
            };

            this.Button3.Click += this.Button3_Click;

            this.Button4 = new Button
            {
                Text = "Server.Close()",
                Left = this.Button3.Left + this.Button3.Width + 8,
                Top = this.Button2.Top,
                Width = this.Button2.Width,
            };

            this.Button4.Click += this.Button4_Click;

            this.Button5 = new Button
            {
                Text = "launch browser",
                Left = this.Button2.Left,
                Top = this.Button2.Top + this.Button2.Height + 8,
                Width = this.Button2.Width,
            };

            this.Button5.Click += this.Button5_Click;

            this.ComboBox1 = new ComboBox
            {
                Left = this.Label3.Left + this.Label3.Width + 8,
                Top = this.TextBox1.Top,
                Width = 48,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = Enumerable.Range(1, Environment.ProcessorCount).Reverse().ToArray(),
            };

            this.ComboBox1.SelectedIndexChanged += this.ComboBox1_SelectedIndexChanged;

            this.NotifyIcon1 = new NotifyIcon
            {
                Text = Application.ProductName,
                Icon = this.Icon,
            };

            this.Controls.AddRange(new Control[] { this.Button1, this.Button2, this.Button3, this.Button4, this.Button5 });
            this.Controls.AddRange(new Control[] { this.TextBox1, this.TextBox2, this.TextBox3 });
            this.Controls.AddRange(new Control[] { this.Label1, this.Label2, this.Label3 });
            this.Controls.AddRange(new Control[] { this.ComboBox1 });

            this.ClientSize = new System.Drawing.Size(this.Button4.Left + this.Button4.Width + 24, this.ClientSize.Height);
            this.MaximizeBox = false;
            this.Text = Application.ProductName;
        }
    }
}