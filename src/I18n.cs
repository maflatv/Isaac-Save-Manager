using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace IsaacSaveManager
{
    /// <summary>
    /// Textos de la interfaz. Por defecto se usa el idioma del sistema; el usuario puede fijar otro
    /// y la elección se guarda en IsaacSaveManager.ini junto al .exe (o en %LOCALAPPDATA% si no se puede escribir ahí).
    /// Los textos están en Strings.&lt;idioma&gt;.cs; si falta una clave se usa el inglés.
    /// </summary>
    public static class I18n
    {
        public const string System = "system";

        public sealed class Language
        {
            public string Code, Name;
        }

        public static readonly Language[] Languages =
        {
            new Language { Code = "es", Name = "Español" },
            new Language { Code = "en", Name = "English" },
            new Language { Code = "fr", Name = "Français" },
            new Language { Code = "pt", Name = "Português" },
            new Language { Code = "ru", Name = "Русский" },
        };

        static readonly Dictionary<string, Dictionary<string, string>> Tables = new Dictionary<string, Dictionary<string, string>>
        {
            ["es"] = Strings.Es,
            ["en"] = Strings.En,
            ["fr"] = Strings.Fr,
            ["pt"] = Strings.Pt,
            ["ru"] = Strings.Ru,
        };

        /// <summary>"system" o el código de un idioma.</summary>
        public static string Choice { get; private set; } = System;

        public static string Lang => Choice == System ? SystemLang : Choice;

        public static string SystemLang
        {
            get
            {
                string c = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                return Tables.ContainsKey(c) ? c : "en";
            }
        }

        public static string NameOf(string code) => Languages.First(l => l.Code == code).Name;

        public static string T(string key, params object[] args)
        {
            string s;
            if (!Tables[Lang].TryGetValue(key, out s) && !Strings.En.TryGetValue(key, out s)) return key;
            return args.Length > 0 ? string.Format(s, args) : s;
        }

        public static string VersionName(GameVersion v) => v == GameVersion.RepentancePlus ? "Repentance+" : "Repentance";

        // ---------- preferencia guardada ----------

        const string SettingsName = "IsaacSaveManager.ini";

        static IEnumerable<string> SettingsCandidates()
        {
            yield return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SettingsName);
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IsaacSaveManager", SettingsName);
        }

        public static void LoadChoice()
        {
            foreach (var path in SettingsCandidates())
            {
                try
                {
                    foreach (var line in File.ReadAllLines(path))
                    {
                        var parts = line.Split(new[] { '=' }, 2);
                        if (parts.Length == 2 && parts[0].Trim() == "Language")
                        {
                            string v = parts[1].Trim();
                            if (v == System || Tables.ContainsKey(v)) { Choice = v; return; }
                        }
                    }
                }
                catch { /* no existe o no se puede leer: probar el siguiente */ }
            }
        }

        public static void SetChoice(string choice)
        {
            Choice = choice == System || Tables.ContainsKey(choice) ? choice : System;
            foreach (var path in SettingsCandidates())
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, "[IsaacSaveManager]\r\nLanguage=" + Choice + "\r\n");
                    return;
                }
                catch { /* carpeta sin permiso de escritura: probar la siguiente */ }
            }
        }
    }
}
