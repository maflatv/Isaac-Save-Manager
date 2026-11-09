using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IsaacSaveManager
{
    /// <summary>Acerca de: icono, versión, autor, Ko-fi y créditos.</summary>
    sealed class AboutForm : Form
    {
        public AboutForm()
        {
            Font = new Font("Segoe UI", 9.5F);
            AutoScaleMode = AutoScaleMode.Dpi;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Text = I18n.T("about.title");
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { /* sin icono */ }

            const int width = 440;
            // El icono es pixel art: se amplía sin suavizar.
            var sprite = AppInfo.IconImage(64);
            var pic = new PictureBox { Size = new Size(96, 96), Margin = new Padding(3, 3, 14, 3) };
            pic.Paint += (s, e) =>
            {
                if (sprite == null) return;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.DrawImage(sprite, new Rectangle(0, 0, pic.Width, pic.Height));
            };

            var head = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            head.Controls.Add(new Label { AutoSize = true, Text = I18n.T("app.title"), Font = new Font("Segoe UI", 16F, FontStyle.Bold) });
            head.Controls.Add(new Label { AutoSize = true, Text = I18n.T("about.version", AppInfo.Version), ForeColor = SystemColors.GrayText });
            head.Controls.Add(new Label { AutoSize = true, Text = I18n.T("about.author"), Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(3, 8, 3, 3) });
            var top = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            top.Controls.Add(pic);
            top.Controls.Add(head);

            var desc = new Label { AutoSize = true, MaximumSize = new Size(width, 0), Text = I18n.T("about.desc"), Margin = new Padding(3, 12, 3, 6) };
            var kofiText = new Label { AutoSize = true, MaximumSize = new Size(width, 0), Text = I18n.T("about.kofi"), Margin = new Padding(3, 8, 3, 6) };
            var kofiBtn = new Button
            {
                AutoSize = true, Text = I18n.T("about.kofiBtn"), Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(41, 171, 224), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Padding = new Padding(10, 4, 10, 4),
            };
            kofiBtn.FlatAppearance.BorderSize = 0;
            kofiBtn.Click += (s, e) => AppInfo.OpenKofi();
            var kofiLink = new LinkLabel { AutoSize = true, Text = AppInfo.KofiUrl, Margin = new Padding(3, 6, 3, 3) };
            kofiLink.LinkClicked += (s, e) => AppInfo.OpenKofi();
            var followText = new Label { AutoSize = true, Text = I18n.T("about.follow"), Margin = new Padding(3, 12, 3, 0), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            var socials = AppInfo.SocialLinks();
            var repo = new LinkLabel { AutoSize = true, Text = I18n.T("about.source"), Margin = new Padding(3, 8, 3, 3) };
            repo.LinkClicked += (s, e) => AppInfo.Open(AppInfo.RepoUrl);
            var credits = new Label { AutoSize = true, MaximumSize = new Size(width, 0), Text = I18n.T("about.credits"), ForeColor = SystemColors.GrayText, Margin = new Padding(3, 16, 3, 6) };
            var close = new Button { AutoSize = true, Text = I18n.T("guide.close"), Anchor = AnchorStyles.Right };
            close.Click += (s, e) => Close();
            CancelButton = close;
            AcceptButton = close;

            var root = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Padding = new Padding(18) };
            root.Controls.Add(top);
            root.Controls.Add(desc);
            root.Controls.Add(kofiText);
            root.Controls.Add(kofiBtn);
            root.Controls.Add(kofiLink);
            root.Controls.Add(followText);
            root.Controls.Add(socials);
            root.Controls.Add(repo);
            root.Controls.Add(credits);
            root.Controls.Add(close);
            Controls.Add(root);
        }
    }
}
