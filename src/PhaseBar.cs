using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IsaacSaveManager
{
    /// <summary>
    /// Las tres fases del cambio de guardados con Steam Cloud: apagar la nube, gestionar archivos, encenderla y jugar.
    /// Marca las fases hechas (verde), la actual (naranja) y las pendientes (gris). Cada fase se puede pulsar.
    /// </summary>
    sealed class PhaseBar : Control
    {
        const int Gap = 26;
        int current;

        /// <summary>0..2 = fase actual; 3 = todas hechas.</summary>
        public int Current
        {
            get => current;
            set { current = value; Invalidate(); }
        }

        public event Action<int> PhaseClicked;

        public PhaseBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Height = 48;
            Cursor = Cursors.Hand;
        }

        protected override Size DefaultSize => new Size(600, 48);

        Rectangle[] Boxes()
        {
            int w = (ClientSize.Width - 2 * Gap - 2) / 3;
            var r = new Rectangle[3];
            for (int i = 0; i < 3; i++) r[i] = new Rectangle(1 + i * (w + Gap), 2, w, ClientSize.Height - 5);
            return r;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? SystemColors.Control);
            var boxes = Boxes();
            for (int i = 0; i < 3; i++)
            {
                bool done = i < current, now = i == current;
                Color fill = done ? Color.FromArgb(226, 244, 227) : now ? Color.FromArgb(255, 240, 219) : Color.FromArgb(246, 246, 248);
                Color border = done ? Color.FromArgb(76, 175, 80) : now ? Color.FromArgb(230, 126, 34) : Color.FromArgb(205, 205, 210);
                Color text = done ? Color.FromArgb(30, 110, 40) : now ? Color.Black : Color.Gray;
                using (var path = Round(boxes[i], 6))
                {
                    using (var b = new SolidBrush(fill)) g.FillPath(b, path);
                    using (var p = new Pen(border, now ? 2.5f : 1.5f)) g.DrawPath(p, path);
                }
                string label = (done ? "✔ " : (i + 1) + ". ") + I18n.T("phase." + (i + 1));
                using (var f = new Font("Segoe UI", 9.5F, now ? FontStyle.Bold : FontStyle.Regular))
                using (var b = new SolidBrush(text))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                    g.DrawString(label, f, b, Rectangle.Inflate(boxes[i], -6, -2), sf);
                if (i < 2)
                {
                    int ax = boxes[i].Right + Gap / 2, ay = boxes[i].Y + boxes[i].Height / 2;
                    using (var b = new SolidBrush(Color.FromArgb(160, 160, 168)))
                        g.FillPolygon(b, new[] { new Point(ax - 5, ay - 7), new Point(ax + 5, ay), new Point(ax - 5, ay + 7) });
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var boxes = Boxes();
            for (int i = 0; i < 3; i++) if (boxes[i].Contains(e.Location)) PhaseClicked?.Invoke(i);
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
    }
}
