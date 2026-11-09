using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace IsaacSaveManager
{
    public enum LocationKind { Steam, Documents, Custom }

    public sealed class SaveLocation
    {
        public LocationKind Kind;
        public string Id;            // Se usa en los nombres de las copias: steam-123, local, custom-xxxx.
        public string AccountId, PersonaName, AccountDir;
        public bool MostRecent;
        public string Folder;        // Steam y Custom: una sola carpeta para las dos versiones.
        public string DocsRep, DocsRepPlus;

        public string FolderFor(GameVersion v) =>
            Kind == LocationKind.Documents ? (v == GameVersion.RepentancePlus ? DocsRepPlus : DocsRep) : Folder;

        public string PathFor(GameVersion v, int slot) => Path.Combine(FolderFor(v), FileName(v, slot));

        public static string FileName(GameVersion v, int slot) =>
            (v == GameVersion.RepentancePlus ? "rep+" : "rep_") + "persistentgamedata" + slot + ".dat";

        public override string ToString()
        {
            switch (Kind)
            {
                case LocationKind.Steam:
                    return I18n.T("loc.steam", string.IsNullOrEmpty(PersonaName) ? "?" : PersonaName, AccountId)
                        + (MostRecent ? I18n.T("loc.recent") : "");
                case LocationKind.Documents: return I18n.T("loc.local");
                default: return I18n.T("loc.custom", Folder);
            }
        }
    }

    public static class Locations
    {
        const string AppId = "250900";
        const long SteamId64Base = 76561197960265728;

        // El juego escribe en %USERPROFILE%\Documents aunque Windows redirija «Documentos» (por ejemplo a OneDrive):
        // se busca ahí primero y, si no existe, en la carpeta redirigida.
        public static string DocsGameFolder(GameVersion v)
        {
            string name = v == GameVersion.RepentancePlus ? "Binding of Isaac Repentance+" : "Binding of Isaac Repentance";
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "My Games", name),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", name),
            };
            return candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];
        }

        public static List<SaveLocation> Find()
        {
            var list = new List<SaveLocation>();
            string steam = SteamPath();
            if (steam != null)
            {
                var users = SteamUsers(steam);
                string userdata = Path.Combine(steam, "userdata");
                string[] accounts = new string[0];
                try { accounts = Directory.GetDirectories(userdata); } catch { /* Steam sin usuarios */ }
                foreach (var dir in accounts)
                {
                    if (!Directory.Exists(Path.Combine(dir, AppId))) continue;
                    string id = Path.GetFileName(dir);
                    Tuple<string, bool> u;
                    users.TryGetValue(id, out u);
                    list.Add(new SaveLocation
                    {
                        Kind = LocationKind.Steam, Id = "steam-" + id, AccountId = id,
                        PersonaName = u?.Item1, MostRecent = u != null && u.Item2, AccountDir = dir,
                        Folder = Path.Combine(dir, AppId, "remote"),
                    });
                }
            }
            list.Sort((a, b) => b.MostRecent.CompareTo(a.MostRecent));
            list.Add(new SaveLocation
            {
                Kind = LocationKind.Documents, Id = "local",
                DocsRep = DocsGameFolder(GameVersion.Repentance),
                DocsRepPlus = DocsGameFolder(GameVersion.RepentancePlus),
            });
            return list;
        }

        public static SaveLocation Custom(string folder) => new SaveLocation
        {
            Kind = LocationKind.Custom, Folder = folder,
            Id = "custom-" + ((uint)folder.ToLowerInvariant().GetHashCode()).ToString("x8"),
        };

        /// <summary>Valor de SteamCloud en el options.ini del juego; null si no se puede leer.</summary>
        public static bool? CloudEnabled(GameVersion v)
        {
            try
            {
                foreach (var line in File.ReadAllLines(Path.Combine(DocsGameFolder(v), "options.ini")))
                {
                    var m = Regex.Match(line, @"^\s*SteamCloud\s*=\s*(\d)");
                    if (m.Success) return m.Groups[1].Value != "0";
                }
            }
            catch { /* el juego nunca se abrió */ }
            return null;
        }

        /// <summary>
        /// Interruptor de Steam Cloud de Isaac en esta cuenta (Propiedades > General), leído de
        /// userdata/&lt;cuenta&gt;/7/remote/sharedconfig.vdf: apps/250900/cloudenabled. Steam lo activa por defecto.
        /// Solo lectura: el usuario lo cambia en Steam. Devuelve null si no se puede leer el archivo.
        /// </summary>
        public static bool? SteamCloudForIsaac(SaveLocation loc)
        {
            if (loc.Kind != LocationKind.Steam) return false;
            try
            {
                string text = File.ReadAllText(Path.Combine(loc.AccountDir, "7", "remote", "sharedconfig.vdf"));
                var m = Regex.Match(text, "\"" + AppId + "\"\\s*\\{[^{}]*?\"cloudenabled\"\\s*\"(\\d+)\"", RegexOptions.IgnoreCase);
                return !m.Success || m.Groups[1].Value != "0";
            }
            catch { return null; }
        }

        static string SteamPath()
        {
            foreach (var (hive, key, value) in new[]
            {
                (Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath"),
                (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath"),
                (Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath"),
            })
            {
                try
                {
                    using (var k = hive.OpenSubKey(key))
                    {
                        var p = k?.GetValue(value) as string;
                        if (!string.IsNullOrEmpty(p) && Directory.Exists(p)) return Path.GetFullPath(p);
                    }
                }
                catch { /* sin permiso o sin Steam */ }
            }
            return null;
        }

        // accountId -> (nombre, última sesión), leído de config/loginusers.vdf.
        static Dictionary<string, Tuple<string, bool>> SteamUsers(string steam)
        {
            var r = new Dictionary<string, Tuple<string, bool>>();
            try
            {
                string text = File.ReadAllText(Path.Combine(steam, "config", "loginusers.vdf"));
                foreach (Match m in Regex.Matches(text, "\"(\\d{17})\"\\s*\\{([^}]*)\\}"))
                {
                    long id64 = long.Parse(m.Groups[1].Value);
                    string body = m.Groups[2].Value;
                    var name = Regex.Match(body, "\"PersonaName\"\\s*\"([^\"]*)\"", RegexOptions.IgnoreCase);
                    var recent = Regex.Match(body, "\"MostRecent\"\\s*\"1\"", RegexOptions.IgnoreCase);
                    r[(id64 - SteamId64Base).ToString()] = Tuple.Create(name.Success ? name.Groups[1].Value : null, recent.Success);
                }
            }
            catch { /* archivo inexistente o ilegible */ }
            return r;
        }
    }
}
