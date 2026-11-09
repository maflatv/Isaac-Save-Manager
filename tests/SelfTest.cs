// Pruebas del núcleo (sin interfaz). Uso: SelfTest.exe <carpeta o archivo>...
// Solo lee los guardados que recibe; las escrituras se prueban en una carpeta temporal.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IsaacSaveManager;

static class SelfTest
{
    static int failures;

    static void Check(bool ok, string what)
    {
        if (!ok) { failures++; Console.WriteLine("  FALLO: " + what); }
    }

    static int Main(string[] args)
    {
        var files = new List<string>();
        foreach (var a in args)
            if (Directory.Exists(a)) files.AddRange(Directory.GetFiles(a, "*persistentgamedata*.dat"));
            else files.Add(a);

        SaveFile anyRep = null, anyPlus = null;
        foreach (var f in files)
        {
            SaveFile s;
            try { s = SaveFile.Load(f); }
            catch (SaveFormatException ex) { Console.WriteLine("rechazado (" + ex.Key + "): " + f); continue; }
            var sum = s.Summarize();
            Console.WriteLine($"{s.Version,-15} {sum.Achievements}/{sum.AchievementsTotal} DG={sum.DeadGod} {f}");
            Check(s.Build().SequenceEqual(s.Raw), "rearmar no da el mismo archivo: " + f);
            ConversionLoss loss;
            if (s.Version == GameVersion.Repentance)
            {
                anyRep = anyRep ?? s;
                var plus = s.ConvertTo(GameVersion.RepentancePlus, out loss);
                Check(!loss.Any, "Rep->Rep+ no debería perder nada");
                var p = SaveFile.Parse(plus.Raw);
                Check(p.Version == GameVersion.RepentancePlus, "Rep->Rep+ no da Rep+");
                Check(p.Block(1).Count == SaveFile.RepPlusAchievements && p.Block(2).Count == SaveFile.RepPlusCounters, "tamaños Rep+");
                Check(p.Summarize().Achievements == sum.Achievements, "Rep->Rep+ cambia los logros");
                Check(plus.ConvertTo(GameVersion.Repentance, out loss).Raw.SequenceEqual(s.Raw), "Rep->Rep+->Rep no es idéntico");
            }
            else
            {
                anyPlus = anyPlus ?? s;
                var rep = SaveFile.Parse(s.ConvertTo(GameVersion.Repentance, out loss).Raw);
                Console.WriteLine($"    a Rep pierde {loss.Achievements} logros y {loss.Counters} contadores");
                Check(rep.Version == GameVersion.Repentance, "Rep+->Rep no da Rep");
                Check(rep.Block(1).Count == SaveFile.RepAchievements && rep.Block(2).Count == SaveFile.RepCounters, "tamaños Rep");
                var back = SaveFile.Parse(rep.ConvertTo(GameVersion.RepentancePlus, out loss).Raw);
                Check(back.Raw.Length == s.Raw.Length || s.Block(1).Count != SaveFile.RepPlusAchievements, "Rep+->Rep->Rep+ cambia de tamaño");
            }
        }

        if (anyPlus != null)
        {
            var bad = (byte[])anyPlus.Raw.Clone();
            bad[0x300] ^= 1;
            Check(Rejects(bad) == "err.checksum", "un byte cambiado debería fallar el checksum");
            Check(Rejects(anyPlus.Raw.Take(anyPlus.Raw.Length - 100).ToArray()) != null, "un archivo cortado debería rechazarse");
            Check(Rejects(new byte[] { 1, 2, 3 }) == "err.notSave", "basura debería ser err.notSave");
        }

        if (anyRep != null && anyPlus != null)
        {
            string tmp = Path.Combine(Path.GetTempPath(), "isaac-save-manager-test-" + Guid.NewGuid().ToString("N"));
            try
            {
                string target = Path.Combine(tmp, "remote", "rep+persistentgamedata2.dat"), backups = Path.Combine(tmp, "backups");
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.WriteAllBytes(target, anyPlus.Raw);
                ConversionLoss loss;
                string backup = SaveWriter.Write(target, anyRep.ConvertTo(GameVersion.RepentancePlus, out loss), backups, "test");
                Check(backup != null && File.ReadAllBytes(backup).SequenceEqual(anyPlus.Raw), "la copia de seguridad no es el archivo anterior");
                var written = SaveFile.Load(target);
                Check(written.Version == GameVersion.RepentancePlus, "lo escrito no es Rep+");
                Check(written.Summarize().Achievements == anyRep.Summarize().Achievements, "lo escrito no tiene los logros del origen");
                Check(!File.Exists(target + ".tmp"), "quedó el .tmp");
                string fresh = Path.Combine(tmp, "remote", "rep_persistentgamedata3.dat");
                Check(SaveWriter.Write(fresh, anyRep, backups, "test") == null && File.ReadAllBytes(fresh).SequenceEqual(anyRep.Raw), "escribir en ranura vacía");
                Console.WriteLine("escritura en carpeta temporal: probada");
            }
            finally { Directory.Delete(tmp, true); }
        }

        Console.WriteLine(failures == 0 ? "TODO OK" : failures + " FALLOS");
        return failures == 0 ? 0 : 1;
    }

    static string Rejects(byte[] d)
    {
        try { SaveFile.Parse(d); return null; }
        catch (SaveFormatException ex) { return ex.Key; }
    }
}
