using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

namespace IsaacSaveManager
{
    /// <summary>Datos del programa que comparten la ventana principal, la ayuda y «Acerca de».</summary>
    static class AppInfo
    {
        public const string KofiUrl = "https://ko-fi.com/maflatv";
        public const string RepoUrl = "https://github.com/maflatv/Isaac-Save-Manager";

        /// <summary>Redes sociales de MaflaTV: se muestran junto al enlace de Ko-fi.</summary>
        public static readonly Tuple<string, string>[] Socials =
        {
            Tuple.Create("YouTube", "https://www.youtube.com/@MaflaTV"),
            Tuple.Create("Twitch", "https://www.twitch.tv/maflatv"),
            Tuple.Create("TikTok", "https://www.tiktok.com/@maflatv"),
            Tuple.Create("Instagram", "https://www.instagram.com/mafla.tv/"),
            Tuple.Create("GitHub", "https://github.com/maflatv"),
        };

        public static void Open(string url)
        {
            try { Process.Start(url); } catch { /* sin navegador predeterminado */ }
        }

        public static string Version
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return v.Major + "." + v.Minor + "." + v.Build;
            }
        }

        public static void OpenKofi() => Open(KofiUrl);

        /// <summary>
        /// Sprite de un objeto del juego para un botón (src/icons/&lt;nombre&gt;.png, incrustado). Se recorta el borde
        /// transparente y se amplía por un factor entero, sin suavizar, para que el pixel art se vea nítido.
        /// </summary>
        public static Bitmap Sprite(string name, int scale)
        {
            try
            {
                using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("IsaacSaveManager.icons." + name + ".png"))
                {
                    if (s == null) return null;
                    using (var src = new Bitmap(s))
                    {
                        int minX = src.Width, minY = src.Height, maxX = -1, maxY = -1;
                        for (int y = 0; y < src.Height; y++)
                            for (int x = 0; x < src.Width; x++)
                                if (src.GetPixel(x, y).A > 16)
                                {
                                    if (x < minX) minX = x; if (x > maxX) maxX = x;
                                    if (y < minY) minY = y; if (y > maxY) maxY = y;
                                }
                        if (maxX < 0) return null;
                        int w = maxX - minX + 1, h = maxY - minY + 1;
                        var dst = new Bitmap(w * scale, h * scale);
                        using (var g = Graphics.FromImage(dst))
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                            g.DrawImage(src, new Rectangle(0, 0, dst.Width, dst.Height), minX, minY, w, h, GraphicsUnit.Pixel);
                        }
                        return dst;
                    }
                }
            }
            catch { return null; }
        }

        /// <summary>Fila de enlaces «YouTube · Twitch · …» para las ventanas de ayuda y Acerca de.</summary>
        public static System.Windows.Forms.FlowLayoutPanel SocialLinks()
        {
            var row = new System.Windows.Forms.FlowLayoutPanel { AutoSize = true, WrapContents = true, Margin = new System.Windows.Forms.Padding(0) };
            foreach (var s in Socials)
            {
                var link = new System.Windows.Forms.LinkLabel { AutoSize = true, Text = s.Item1, Margin = new System.Windows.Forms.Padding(3, 3, 8, 3) };
                string url = s.Item2;
                link.LinkClicked += (o, e) => Open(url);
                row.Controls.Add(link);
            }
            return row;
        }

        /// <summary>
        /// El icono del programa (Moving Box) al tamaño pedido, leído del recurso incrustado. El .ico guarda cada tamaño
        /// como PNG, y System.Drawing.Icon no los convierte bien, así que se busca la entrada y se lee el PNG directamente.
        /// </summary>
        public static Bitmap IconImage(int size)
        {
            try
            {
                using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("IsaacSaveManager.app.ico"))
                {
                    if (s == null) return null;
                    var data = new byte[s.Length];
                    s.Read(data, 0, data.Length);
                    int count = BitConverter.ToUInt16(data, 4);
                    for (int i = 0; i < count; i++)
                    {
                        int e = 6 + 16 * i;
                        int w = data[e] == 0 ? 256 : data[e];
                        if (w != size) continue;
                        int len = BitConverter.ToInt32(data, e + 8), off = BitConverter.ToInt32(data, e + 12);
                        using (var ms = new System.IO.MemoryStream(data, off, len))
                            return new Bitmap(Image.FromStream(ms));
                    }
                }
            }
            catch { /* recurso dañado: se muestra sin icono */ }
            return null;
        }
    }
}
