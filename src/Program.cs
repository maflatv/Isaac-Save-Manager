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
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            I18n.LoadChoice();
            Application.Run(new MainForm());
        }
    }
}
