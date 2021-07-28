namespace HttpListening
{
    using System;
    using System.Windows.Forms;

    internal partial class Form1 : Form
    {
        private void InitializeComponent()
        {
            this.Button1 = new Button
            {
                Text = "Server.Start()",
                Left = 8,
                Top = 8,
                Width = 100,
            };

            this.Button2 = new Button
            {
                Text = "Server.Stop()",
                Left = this.Button1.Left + this.Button1.Width + 8,
                Top = this.Button1.Top,
                Width = this.Button1.Width,
            };

            this.Button3 = new Button
            {
                Text = "Server.Close()",
                Left = this.Button2.Left + this.Button2.Width + 8,
                Top = this.Button1.Top,
                Width = this.Button1.Width,
            };

            this.Button4 = new Button
            {
                Text = "launch browser",
                Left = this.Button1.Left,
                Top = this.Button1.Top + this.Button1.Height + 8,
                Width = this.Button1.Width,
            };

            this.TextBox1 = new TextBox
            {
                Left = this.Button1.Left,
                Top = this.Button4.Top + this.Button4.Height + 8,
                Width = 300,
                Height = 100,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
            };

            this.Controls.AddRange(new Control[] { this.Button1, this.Button2, this.Button3, this.Button4, this.TextBox1 });
            this.ClientSize = new System.Drawing.Size(this.Button3.Left + this.Button3.Width + 24, this.ClientSize.Height);
            this.Text = Application.ProductName;
        }
    }
}