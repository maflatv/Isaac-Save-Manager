using System;
using System.Reflection;
using System.Windows.Forms;

[assembly: AssemblyTitle("Isaac Save Manager")]
[assembly: AssemblyDescription("Gestiona y convierte guardados de The Binding of Isaac: Repentance / Repentance+")]
[assembly: AssemblyProduct("Isaac Save Manager")]
[assembly: AssemblyCopyright("MaflaTV")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace IsaacSaveManager
{
    static class Program
    {
        /// <summary>Carpeta pasada con --folder: se añade como ubicación y se selecciona al arrancar.</summary>
        public static string StartFolder;

        // Opciones: --folder "<carpeta con guardados>"  y  --lang es|en|fr|pt|ru (solo para esta sesión).
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            I18n.LoadChoice();
            for (int i = 0; i + 1 < args.Length; i++)
            {
                if (args[i] == "--folder") StartFolder = System.IO.Path.GetFullPath(args[++i]);
                else if (args[i] == "--lang") I18n.UseForSession(args[++i]);
            }
            Application.Run(new MainForm());
        }
    }
}
