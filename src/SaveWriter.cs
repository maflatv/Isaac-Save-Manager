using System;
using System.Diagnostics;
using System.IO;

namespace IsaacSaveManager
{
    /// <summary>Escritura en disco sin interfaz: copia de seguridad, archivo temporal, verificación y reemplazo.</summary>
    public static class SaveWriter
    {
        /// <summary>Escribe data en path. Si ya hay un archivo, antes lo copia a backupDir. Devuelve la ruta de la copia, o null.</summary>
        public static string Write(string path, SaveFile data, string backupDir, string tag)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string backup = File.Exists(path) ? Backup(path, backupDir, tag) : null;
            string tmp = path + ".tmp";
            File.WriteAllBytes(tmp, data.Raw);
            try { SaveFile.Parse(File.ReadAllBytes(tmp)); }
            catch { File.Delete(tmp); throw; }
            if (File.Exists(path)) File.Replace(tmp, path, null, true);
            else File.Move(tmp, path);
            return backup;
        }

        public static string Backup(string path, string backupDir, string tag)
        {
            Directory.CreateDirectory(backupDir);
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string dest = Path.Combine(backupDir, stamp + "_" + tag + "_" + Path.GetFileName(path));
            for (int i = 2; File.Exists(dest); i++)
                dest = Path.Combine(backupDir, stamp + "-" + i + "_" + tag + "_" + Path.GetFileName(path));
            File.Copy(path, dest);
            return dest;
        }

        public static bool GameRunning() => Running("isaac-ng");
        public static bool SteamRunning() => Running("steam");

        static bool Running(string name)
        {
            var ps = Process.GetProcessesByName(name);
            foreach (var p in ps) p.Dispose();
            return ps.Length > 0;
        }
    }
}
