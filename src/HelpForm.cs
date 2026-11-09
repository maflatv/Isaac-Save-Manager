using System;
using System.Drawing;
using System.Windows.Forms;

namespace IsaacSaveManager
{
    /// <summary>Ayuda: lista de preguntas frecuentes a la izquierda y la respuesta a la derecha.</summary>
    sealed class HelpForm : Form
    {
        public const int Questions = 10;
        public const int CloudQuestion = 5; // «¿Por qué hay que apagar Steam Cloud?»

        readonly ListBox list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false, Font = new Font("Segoe UI", 10F), DrawMode = DrawMode.OwnerDrawVariable };
        readonly RichTextBox answer = new RichTextBox
        {
            Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = SystemColors.Window,
            Font = new Font("Segoe UI", 10.5F), DetectUrls = false,
        };
        readonly Button btnGuide = new Button { AutoSize = true }, btnKofi = new Button { AutoSize = true }, btnClose = new Button { AutoSize = true };

        /// <summary>Se pulsa «Abrir la guía ilustrada»: la ventana principal la abre.</summary>
        public event Action OpenGuideRequested;

        public HelpForm(int question = 1)
        {
            Font = new Font("Segoe UI", 9.5F);
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(900, 560);
            MinimumSize = new Size(700, 420);
            ShowInTaskbar = false;
            MinimizeBox = false;
            Text = I18n.T("help.title");
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { /* sin icono */ }

            // Preguntas largas: varias líneas por elemento. Los elementos se añaden en Load, cuando la lista
            // ya tiene su ancho final; si no, las alturas se calculan con un ancho mínimo y salen enormes.
            list.MeasureItem += (s, e) =>
            {
                var size = TextRenderer.MeasureText(list.Items[e.Index].ToString(), list.Font, new Size(list.ClientSize.Width - 16, 0), TextFormatFlags.WordBreak);
                e.ItemHeight = Math.Min(255, size.Height + 16);
            };
            list.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                e.DrawBackground();
                bool sel = (e.State & DrawItemState.Selected) != 0;
                var r = new Rectangle(e.Bounds.X + 8, e.Bounds.Y + 7, e.Bounds.Width - 16, e.Bounds.Height - 7);
                TextRenderer.DrawText(e.Graphics, list.Items[e.Index].ToString(), list.Font, r, sel ? SystemColors.HighlightText : SystemColors.ControlText, TextFormatFlags.WordBreak);
            };
            list.SelectedIndexChanged += (s, e) => ShowQuestion(list.SelectedIndex + 1);

            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };
            split.Panel1.Controls.Add(list);
            split.Panel2.Controls.Add(answer);
            split.Panel2.Padding = new Padding(14, 8, 8, 8);
            split.Panel2.BackColor = SystemColors.Window;

            btnGuide.Text = I18n.T("help.openGuide");
            btnKofi.Text = I18n.T("about.kofiBtn");
            btnClose.Text = I18n.T("guide.close");
            btnGuide.Click += (s, e) => OpenGuideRequested?.Invoke();
            btnKofi.Click += (s, e) => AppInfo.OpenKofi();
            btnClose.Click += (s, e) => Close();
            CancelButton = btnClose;
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            buttons.Controls.AddRange(new Control[] { btnClose, btnKofi, btnGuide });
            // Redes sociales a la izquierda de la barra inferior, botones a la derecha.
            var follow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
            follow.Controls.Add(new Label { AutoSize = true, Text = I18n.T("help.follow"), Margin = new Padding(3, 6, 3, 3) });
            var links = AppInfo.SocialLinks();
            links.Margin = new Padding(0, 3, 0, 0);
            follow.Controls.Add(links);
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8, 6, 8, 6) };
            bottom.Controls.Add(follow);
            bottom.Controls.Add(buttons);

            Controls.Add(split);
            Controls.Add(bottom);
            Load += (s, e) =>
            {
                split.SplitterDistance = (int)(300 * DeviceDpi / 96f);
                for (int i = 1; i <= Questions; i++) list.Items.Add(I18n.T("help.q" + i));
                list.SelectedIndex = Math.Max(0, Math.Min(Questions, question) - 1);
            };
        }

        void ShowQuestion(int q)
        {
            answer.Clear();
            answer.SelectionFont = new Font("Segoe UI", 13F, FontStyle.Bold);
            answer.AppendText(I18n.T("help.q" + q) + "\n\n");
            answer.SelectionFont = new Font("Segoe UI", 10.5F);
            answer.AppendText(I18n.T("help.a" + q));
            answer.SelectionStart = 0;
            btnGuide.Visible = q == CloudQuestion || q == 3;
        }
    }
}
