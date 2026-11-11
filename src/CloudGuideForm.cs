using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IsaacSaveManager
{
    /// <summary>
    /// Guía ilustrada: apagar Steam Cloud de Isaac a mano, gestionar los archivos, volver a encenderlo antes de jugar
    /// y qué elegir si Steam avisa de un conflicto. Las ilustraciones reproducen la interfaz actual de Steam
    /// (página del juego con el engranaje y ventana de Propiedades) dibujadas aquí, para que se traduzcan con el programa.
    /// </summary>
    sealed class CloudGuideForm : Form
    {
        public const int StepTurnOff = 0, StepManage = 3, StepTurnOn = 4;
        const int Steps = 6;
        int step;
        readonly Label lblBanner = new Label { AutoSize = true, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(30, 120, 45), Margin = new Padding(3, 0, 3, 8) };
        readonly Label lblStep = new Label { AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(3, 0, 3, 0) };
        readonly Label lblTitle = new Label { AutoSize = true, Font = new Font("Segoe UI", 13F, FontStyle.Bold), Margin = new Padding(3, 0, 3, 8) };
        readonly Label lblText = new Label { AutoSize = true, Font = new Font("Segoe UI", 10F), Margin = new Padding(3, 10, 3, 10) };
        readonly Canvas canvas = new Canvas { Size = new Size(640, 320), Margin = new Padding(3) };
        readonly Button btnPrev = new Button { AutoSize = true }, btnNext = new Button { AutoSize = true }, btnClose = new Button { AutoSize = true };

        static readonly Color Bg = Color.FromArgb(27, 30, 37), Panel = Color.FromArgb(38, 43, 52), Row = Color.FromArgb(50, 56, 68),
            Sel = Color.FromArgb(61, 111, 182), Txt = Color.FromArgb(225, 230, 238), Dim = Color.FromArgb(150, 158, 172),
            Mark = Color.FromArgb(232, 64, 72), Ok = Color.FromArgb(76, 175, 80), SteamBlue = Color.FromArgb(26, 159, 255);

        public CloudGuideForm(int startStep = 0, bool afterWrite = false)
        {
            Font = new Font("Segoe UI", 9.5F);
            AutoScaleMode = AutoScaleMode.Dpi;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Text = I18n.T("guide.title");
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { /* sin icono */ }

            var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            buttons.Controls.AddRange(new Control[] { btnClose, btnNext, btnPrev });
            var root = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Padding = new Padding(14), Dock = DockStyle.Fill };
            if (afterWrite) { lblBanner.Text = I18n.T("guide.afterWrite"); root.Controls.Add(lblBanner); }
            root.Controls.Add(lblStep);
            root.Controls.Add(lblTitle);
            root.Controls.Add(canvas);
            root.Controls.Add(lblText);
            root.Controls.Add(buttons);
            Controls.Add(root);
            lblText.MaximumSize = new Size(canvas.Width, 0);
            lblTitle.MaximumSize = new Size(canvas.Width, 0);

            btnPrev.Text = I18n.T("guide.prev");
            btnNext.Text = I18n.T("guide.next");
            btnClose.Text = I18n.T("guide.close");
            btnPrev.Click += (s, e) => Go(step - 1);
            btnNext.Click += (s, e) => Go(step + 1);
            btnClose.Click += (s, e) => Close();
            CancelButton = btnClose;
            canvas.Paint += (s, e) => Draw(e.Graphics);
            Go(startStep);
        }

        void Go(int i)
        {
            step = Math.Max(0, Math.Min(Steps - 1, i));
            lblStep.Text = I18n.T("guide.step", step + 1, Steps);
            lblTitle.Text = I18n.T("guide.h" + (step + 1));
            lblText.Text = I18n.T("guide.t" + (step + 1));
            btnPrev.Enabled = step > 0;
            btnNext.Enabled = step < Steps - 1;
            canvas.Invalidate();
        }

        void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.FromArgb(18, 20, 25));
            // Se dibuja en coordenadas de 640x320 y se escala al tamaño real (pantallas con DPI alto).
            g.ScaleTransform(canvas.Width / 640f, canvas.Height / 320f);
            switch (step)
            {
                case 0: DrawGamePage(g); break;
                case 1: DrawProperties(g, false); break;
                case 2: DrawTray(g); break;
                case 3: DrawThisApp(g); break;
                case 4: DrawProperties(g, true); break;
                default: DrawConflict(g); break;
            }
        }

        // Paso 1: página del juego en la Biblioteca > engranaje > Propiedades…
        void DrawGamePage(Graphics g)
        {
            var win = new Rectangle(20, 16, 600, 288);
            Window(g, win, "Steam");
            var side = new Rectangle(win.X + 8, win.Y + 36, 150, win.Height - 44);
            Fill(g, Panel, side, 4);
            Label(g, I18n.T("guide.library").ToUpper(), side.X + 8, side.Y + 8, Dim, 7.5F, FontStyle.Bold);
            string[] games = { "Balatro", "Hades", "The Binding of Isaac: Rebirth", "Terraria" };
            Rectangle isaacRow = Rectangle.Empty;
            for (int i = 0; i < games.Length; i++)
            {
                var r = new Rectangle(side.X + 4, side.Y + 30 + i * 26, side.Width - 8, 22);
                bool isaac = i == 2;
                if (isaac) { Fill(g, Sel, r, 3); isaacRow = r; }
                Fill(g, isaac ? Color.FromArgb(200, 120, 90) : Row, new Rectangle(r.X + 4, r.Y + 4, 14, 14), 2);
                Label(g, games[i], r.X + 22, r.Y + 3, isaac ? Color.White : Txt, 8F, isaac ? FontStyle.Bold : FontStyle.Regular, new SizeF(r.Width - 24, 16));
            }
            Outline(g, Rectangle.Inflate(isaacRow, 3, 3));
            Badge(g, "1", isaacRow.Right - 6, isaacRow.Y - 12, null);

            int px = side.Right + 10;
            var page = new Rectangle(px, win.Y + 36, win.Right - px - 8, win.Height - 44);
            var banner = new Rectangle(page.X, page.Y, page.Width, 84);
            using (var b = new LinearGradientBrush(banner, Color.FromArgb(78, 52, 58), Color.FromArgb(44, 38, 52), 0f)) g.FillRectangle(b, banner);
            Label(g, "The Binding of Isaac: Rebirth", banner.X + 12, banner.Bottom - 26, Color.FromArgb(235, 225, 215), 11F, FontStyle.Bold);

            int sy = banner.Bottom + 10;
            Label(g, I18n.T("guide.playtime").ToUpper(), page.X + 8, sy, Dim, 7F, FontStyle.Bold);
            Label(g, "393.6 h", page.X + 8, sy + 13, Txt, 8F, FontStyle.Regular);
            Label(g, I18n.T("guide.achievements").ToUpper(), page.X + 128, sy, Dim, 7F, FontStyle.Bold);
            Label(g, "641/641", page.X + 128, sy + 13, Txt, 8F, FontStyle.Regular);
            Fill(g, Sel, new Rectangle(page.X + 180, sy + 19, 50, 3), 0);

            var heart = new Rectangle(page.Right - 32, sy - 2, 26, 26);
            var info = new Rectangle(heart.X - 32, heart.Y, 26, 26);
            var gear = new Rectangle(info.X - 32, heart.Y, 26, 26);
            foreach (var t in new[] { Tuple.Create(gear, "⚙"), Tuple.Create(info, "ⓘ"), Tuple.Create(heart, "♥") })
            {
                Fill(g, Row, t.Item1, 3);
                Centered(g, t.Item2, t.Item1, Txt, 11F, FontStyle.Regular, "Segoe UI Symbol");
            }
            Outline(g, Rectangle.Inflate(gear, 3, 3));
            Badge(g, "2", gear.X - 30, gear.Y + 1, null);

            var menu = new Rectangle(heart.Right - 180, gear.Bottom + 6, 180, 4 * 24 + 20);
            Fill(g, Color.FromArgb(62, 70, 84), menu, 4);
            string[] items = { I18n.T("guide.m.fav"), I18n.T("guide.m.addTo") + "   ›", I18n.T("guide.m.manage") + "   ›", "—", I18n.T("guide.m.props") };
            int y = menu.Y + 5;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == "—") { using (var p = new Pen(Dim)) g.DrawLine(p, menu.X + 8, y + 5, menu.Right - 8, y + 5); y += 10; continue; }
                var r = new Rectangle(menu.X + 4, y, menu.Width - 8, 24);
                bool props = i == items.Length - 1;
                if (props) Fill(g, Sel, r, 3);
                Label(g, items[i], r.X + 10, r.Y + 4, props ? Color.White : Txt, 8.5F, props ? FontStyle.Bold : FontStyle.Regular);
                if (props) { Outline(g, Rectangle.Inflate(r, 3, 3)); Badge(g, "3", menu.X - 32, r.Y, null); }
                y += 24;
            }
        }

        // Pasos 2 y 5: ventana de Propiedades > General > recuadro Steam Cloud (apagar en el 2, encender en el 5).
        void DrawProperties(Graphics g, bool turnOn)
        {
            var win = new Rectangle(20, 16, 600, 288);
            Fill(g, Bg, win, 6);
            var nav = new Rectangle(win.X, win.Y, 170, win.Height);
            Fill(g, Color.FromArgb(22, 25, 31), nav, 6);
            Label(g, "—   ☐   ✕", win.Right - 78, win.Y + 6, Dim, 9F, FontStyle.Regular);
            Label(g, "THE BINDING OF ISAAC: REBIRTH", nav.X + 12, nav.Y + 14, SteamBlue, 9.5F, FontStyle.Bold, new SizeF(nav.Width - 20, 40));
            var tabs = I18n.T("guide.tabs").Split('|');
            for (int i = 0; i < tabs.Length; i++)
            {
                var r = new Rectangle(nav.X, nav.Y + 58 + i * 22, nav.Width, 22);
                if (i == 0) Fill(g, Color.FromArgb(52, 58, 70), r, 0);
                Label(g, tabs[i], r.X + 12, r.Y + 3, i == 0 ? Color.White : Txt, 8.5F, FontStyle.Regular, new SizeF(nav.Width - 20, 18));
                if (i == 0) { Outline(g, new Rectangle(r.X + 3, r.Y, r.Width - 6, r.Height)); Badge(g, "1", r.Right - 30, r.Y - 1, null); }
            }

            int x = nav.Right + 18, w = win.Right - x - 16;
            Label(g, tabs[0], x, win.Y + 12, Txt, 13F, FontStyle.Bold);
            var ov = new Rectangle(x, win.Y + 48, w, 30);
            Fill(g, Row, ov, 3);
            Label(g, I18n.T("guide.overlay"), ov.X + 10, ov.Y + 7, Txt, 8.5F, FontStyle.Regular, new SizeF(w - 76, 18));
            Toggle(g, ov.Right - 52, ov.Y + 5, true, false);

            var sec = new Rectangle(x, ov.Bottom + 12, w, 130);
            Fill(g, Panel, sec, 4);
            Label(g, "Steam Cloud", sec.X + 10, sec.Y + 8, Txt, 10F, FontStyle.Bold);
            Label(g, I18n.T("guide.cloudDesc"), sec.X + 10, sec.Y + 28, Dim, 7.5F, FontStyle.Regular, new SizeF(sec.Width - 20, 28));
            var row = new Rectangle(sec.X + 6, sec.Y + 60, sec.Width - 12, 40);
            Fill(g, Row, row, 3);
            Label(g, I18n.T("guide.keepCloud"), row.X + 8, row.Y + 4, Txt, 8.5F, FontStyle.Regular, new SizeF(row.Width - 100, 34));
            Toggle(g, row.Right - 52, row.Y + 9, turnOn, true);
            Label(g, I18n.T("guide.usage"), sec.X + 10, sec.Y + 108, Dim, 7.5F, FontStyle.Regular, new SizeF(sec.Width - 20, 16));
            Outline(g, Rectangle.Inflate(sec, 3, 3));
            Badge(g, "2", sec.X + 112, sec.Y + 5, I18n.T(turnOn ? "guide.turnOn" : "guide.turnOff"));
            Label(g, I18n.T(turnOn ? "guide.mustBeOn" : "guide.mustBeOff"), x, sec.Bottom + 12, Ok, 9F, FontStyle.Bold, new SizeF(w, 36));
        }

        // Paso 3: cerrar Steam del todo desde el icono junto al reloj.
        void DrawTray(Graphics g)
        {
            var desk = new Rectangle(20, 16, 600, 288);
            Fill(g, Color.FromArgb(40, 64, 96), desk, 6);
            var bar = new Rectangle(desk.X, desk.Bottom - 36, desk.Width, 36);
            Fill(g, Color.FromArgb(30, 32, 38), bar, 0);
            Label(g, "12:00", bar.Right - 50, bar.Y + 10, Txt, 9F, FontStyle.Regular);
            var icon = new Rectangle(bar.Right - 110, bar.Y + 8, 20, 20);
            using (var b = new SolidBrush(Color.FromArgb(200, 210, 225))) g.FillEllipse(b, icon);
            using (var b = new SolidBrush(Color.FromArgb(30, 32, 38))) g.FillEllipse(b, Rectangle.Inflate(icon, -6, -6));
            Outline(g, Rectangle.Inflate(icon, 5, 5));
            Badge(g, "1", icon.X - 30, icon.Y - 2, null);
            Label(g, I18n.T("guide.rightClickIcon"), icon.X - 240, icon.Y + 1, Color.White, 8.5F, FontStyle.Bold, new SizeF(205, 18));

            var menu = new Rectangle(bar.Right - 250, desk.Y + 28, 200, 190);
            Fill(g, Color.FromArgb(62, 70, 84), menu, 4);
            string[] items = { I18n.T("guide.library"), I18n.T("guide.tray.store"), I18n.T("guide.tray.friends"), I18n.T("guide.tray.settings"), "—", I18n.T("guide.tray.exit") };
            for (int i = 0; i < items.Length; i++)
            {
                var r = new Rectangle(menu.X + 4, menu.Y + 6 + i * 30, menu.Width - 8, 28);
                if (items[i] == "—") { using (var p = new Pen(Dim)) g.DrawLine(p, r.X + 6, r.Y + 14, r.Right - 6, r.Y + 14); continue; }
                bool exit = i == items.Length - 1;
                if (exit) { Fill(g, Sel, r, 3); Outline(g, Rectangle.Inflate(r, 4, 4)); Badge(g, "2", r.X - 30, r.Y + 1, null); }
                Label(g, items[i], r.X + 10, r.Y + 6, exit ? Color.White : Txt, 9F, exit ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        // Paso 4: este programa: Actualizar, aviso en verde, y gestionar las ranuras.
        void DrawThisApp(Graphics g)
        {
            var win = new Rectangle(20, 16, 600, 288);
            Window(g, win, I18n.T("app.title"));
            var light = new Rectangle(win.X + 8, win.Y + 34, win.Width - 16, win.Height - 42);
            Fill(g, Color.FromArgb(243, 244, 247), light, 3);
            Label(g, I18n.T("lbl.location"), light.X + 12, light.Y + 16, Color.Black, 9F, FontStyle.Regular, new SizeF(100, 18));
            var combo = new Rectangle(light.X + 118, light.Y + 12, 236, 24);
            Fill(g, Color.White, combo, 2);
            g.DrawRectangle(Pens.Gray, combo);
            Label(g, "Steam Cloud: …", combo.X + 6, combo.Y + 4, Color.Black, 9F, FontStyle.Regular);
            var btn = new Rectangle(combo.Right + 12, combo.Y - 1, 110, 26);
            Fill(g, Color.FromArgb(225, 225, 225), btn, 3);
            g.DrawRectangle(Pens.DarkGray, btn);
            Centered(g, I18n.T("btn.refresh"), btn, Color.Black, 9F, FontStyle.Regular);
            Outline(g, Rectangle.Inflate(btn, 5, 5));
            Badge(g, "1", btn.Right + 4, btn.Y - 12, null);

            var ok = new Rectangle(light.X + 12, light.Y + 52, light.Width - 24, 50);
            Fill(g, Color.FromArgb(226, 244, 227), ok, 4);
            Label(g, "✔  " + I18n.T("cloud.isaacOff"), ok.X + 12, ok.Y + 7, Color.FromArgb(30, 110, 40), 9.5F, FontStyle.Bold, new SizeF(ok.Width - 24, ok.Height - 10));
            Outline(g, Rectangle.Inflate(ok, 4, 4));
            Badge(g, "2", ok.Right - 14, ok.Y - 16, null);

            for (int i = 0; i < 3; i++)
            {
                var r = new Rectangle(light.X + 12, light.Y + 114 + i * 26, light.Width - 24, 22);
                Fill(g, i == 0 ? Color.FromArgb(204, 228, 247) : Color.White, r, 0);
                Label(g, "Repentance+   " + (i + 1), r.X + 8, r.Y + 3, Color.Black, 9F, FontStyle.Regular);
            }
            var act = new Rectangle(light.X + 12, light.Bottom - 34, 210, 26);
            Fill(g, Color.FromArgb(225, 225, 225), act, 3);
            g.DrawRectangle(Pens.DarkGray, act);
            Centered(g, I18n.T("btn.replace"), act, Color.Black, 9F, FontStyle.Regular);
            Outline(g, Rectangle.Inflate(act, 4, 4));
            Badge(g, "3", act.Right + 8, act.Y + 1, null);
        }

        // Paso 6: aviso de conflicto de Steam Cloud: elegir la copia de este equipo.
        void DrawConflict(Graphics g)
        {
            var win = new Rectangle(40, 18, 560, 284);
            Window(g, win, I18n.T("guide.c.title"));
            int cw = (win.Width - 48) / 2;
            for (int i = 0; i < 2; i++)
            {
                bool local = i == 0;
                var card = new Rectangle(win.X + 16 + i * (cw + 16), win.Y + 48, cw, 196);
                Fill(g, Panel, card, 6);
                // Icono en la esquina derecha: a la izquierda va la etiqueta «Elige este», que en algunos idiomas es larga.
                Centered(g, local ? "🖳" : "☁", new Rectangle(card.Right - 54, card.Y + 6, 44, 40), local ? Txt : Dim, 22F, FontStyle.Regular, "Segoe UI Symbol");
                Label(g, I18n.T(local ? "guide.c.local" : "guide.c.cloud"), card.X + 14, card.Y + 56, Txt, 10F, FontStyle.Bold, new SizeF(cw - 28, 40));
                Label(g, I18n.T(local ? "guide.c.modLocal" : "guide.c.modCloud"), card.X + 14, card.Y + 98, local ? Ok : Dim, 9F, local ? FontStyle.Bold : FontStyle.Regular, new SizeF(cw - 28, 20));
                var btn = new Rectangle(card.X + 14, card.Bottom - 44, cw - 28, 30);
                Fill(g, local ? Sel : Row, btn, 4);
                Centered(g, I18n.T("guide.c.use"), btn, local ? Color.White : Dim, 9F, FontStyle.Bold);
                if (local)
                {
                    Outline(g, Rectangle.Inflate(card, 4, 4), Ok);
                    Badge(g, "✔", card.X + 10, card.Y + 10, I18n.T("guide.c.pick"), Ok);
                }
                else Label(g, "✕ " + I18n.T("guide.c.old"), card.X + 14, card.Y + 124, Mark, 9F, FontStyle.Bold, new SizeF(cw - 28, 20));
            }
        }

        // ---------- piezas de dibujo ----------

        static void Window(Graphics g, Rectangle r, string title)
        {
            Fill(g, Bg, r, 6);
            Fill(g, Color.FromArgb(20, 23, 28), new Rectangle(r.X, r.Y, r.Width, 28), 6);
            Label(g, title, r.X + 12, r.Y + 6, Txt, 9F, FontStyle.Bold);
            Label(g, "—   ☐   ✕", r.Right - 78, r.Y + 6, Dim, 9F, FontStyle.Regular);
        }

        // Interruptor al estilo de Steam: azul encendido, gris apagado. La etiqueta ON/OFF va a la izquierda.
        static void Toggle(Graphics g, int x, int y, bool on, bool showLabel)
        {
            var r = new Rectangle(x, y, 44, 22);
            Fill(g, on ? Sel : Color.FromArgb(90, 96, 108), r, 11);
            using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, on ? x + 24 : x + 3, y + 3, 16, 16);
            if (showLabel) Label(g, on ? "ON" : "OFF", x - 34, y + 3, Ok, 8.5F, FontStyle.Bold);
        }

        static void Outline(Graphics g, Rectangle r) => Outline(g, r, Mark);

        static void Outline(Graphics g, Rectangle r, Color c)
        {
            using (var p = new Pen(c, 3f)) using (var path = Round(r, 6)) g.DrawPath(p, path);
        }

        static void Badge(Graphics g, string n, int x, int y, string note) => Badge(g, n, x, y, note, Mark);

        static void Badge(Graphics g, string n, int x, int y, string note, Color c)
        {
            var circle = new Rectangle(x, y, 24, 24);
            using (var b = new SolidBrush(c)) g.FillEllipse(b, circle);
            Centered(g, n, circle, Color.White, 9.5F, FontStyle.Bold);
            if (note != null) Label(g, note, x + 28, y + 3, c, 9F, FontStyle.Bold);
        }

        static void Label(Graphics g, string s, int x, int y, Color c, float size, FontStyle style, SizeF? box = null)
        {
            using (var f = new Font("Segoe UI", size, style))
            using (var b = new SolidBrush(c))
            {
                if (!box.HasValue) { g.DrawString(s, f, b, x, y); return; }
                // Una sola línea si la caja es baja (con «…» si no cabe); varias líneas si es alta.
                using (var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter })
                {
                    if (box.Value.Height < 20) sf.FormatFlags |= StringFormatFlags.NoWrap;
                    g.DrawString(s, f, b, new RectangleF(x, y, box.Value.Width, box.Value.Height), sf);
                }
            }
        }

        static void Centered(Graphics g, string s, Rectangle r, Color c, float size, FontStyle style, string family = "Segoe UI")
        {
            using (var f = new Font(family, size, style))
            using (var b = new SolidBrush(c))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                g.DrawString(s, f, b, r, sf);
        }

        static void Fill(Graphics g, Color c, Rectangle r, int radius)
        {
            using (var b = new SolidBrush(c))
            {
                if (radius <= 0) { g.FillRectangle(b, r); return; }
                using (var p = Round(r, radius)) g.FillPath(b, p);
            }
        }

        static GraphicsPath Round(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        sealed class Canvas : Control
        {
            public Canvas() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true); }
        }
    }
}
