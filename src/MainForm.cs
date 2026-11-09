using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace IsaacSaveManager
{
    sealed class SlotRow
    {
        public SaveLocation Location;
        public GameVersion Version;
        public int Slot;
        public string Path;
        public SaveFile File;      // null si no existe o no se puede leer
        public string Error;       // texto ya traducido, o null
        public bool Exists;
    }

    sealed class MainForm : Form
    {
        static readonly GameVersion[] Versions = { GameVersion.RepentancePlus, GameVersion.Repentance };

        readonly ComboBox cbLocation = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 430 };
        readonly ComboBox cbLang = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190 };
        bool fillingLang;
        readonly Label lblLocation = AutoLabel(), lblLang = AutoLabel();
        readonly Label lblHint = new Label { AutoSize = true, Margin = new Padding(3, 6, 3, 6), ForeColor = SystemColors.GrayText };
        readonly Label lblCloud = new Label { AutoSize = true, Margin = new Padding(3, 0, 3, 6), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        readonly Button btnRefresh = Btn(), btnAddFolder = Btn(), btnCloudHelp = Btn(), btnReplace = Btn(), btnCopyTo = Btn(), btnExport = Btn(),
            btnRestore = Btn(), btnConvertFile = Btn(), btnOpenFolder = Btn(), btnBackups = Btn(), btnBackupAll = Btn();
        readonly ListView lv = new ListView
        {
            View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false,
            Dock = DockStyle.Fill, GridLines = true,
        };
        readonly TextBox log = new TextBox
        {
            Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill,
            BackColor = SystemColors.Window,
        };
        readonly ContextMenuStrip copyMenu = new ContextMenuStrip();
        readonly List<SaveLocation> customLocations = new List<SaveLocation>();
        readonly PhaseBar phases = new PhaseBar { Dock = DockStyle.Fill, Margin = new Padding(3, 2, 3, 6) };
        readonly ToolTip tips = new ToolTip();
        // Ubicaciones de Steam en las que se escribió en esta sesión: la fase 3 (encender la nube) queda pendiente.
        readonly HashSet<string> writtenThisSession = new HashSet<string>();
        // Menú «Ayuda» y barra inferior con el enlace de Ko-fi.
        readonly MenuStrip menu = new MenuStrip();
        readonly ToolStripMenuItem mHelp = new ToolStripMenuItem(), mFaq = new ToolStripMenuItem { ShortcutKeys = Keys.F1 },
            mGuide = new ToolStripMenuItem(), mKofi = new ToolStripMenuItem(), mFollow = new ToolStripMenuItem(), mAbout = new ToolStripMenuItem();
        readonly StatusStrip status = new StatusStrip { SizingGrip = false };
        readonly ToolStripStatusLabel kofiLabel = new ToolStripStatusLabel { IsLink = true, Spring = true, TextAlign = ContentAlignment.MiddleRight };

        public MainForm()
        {
            Font = new Font("Segoe UI", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1000, 720);
            MinimumSize = new Size(780, 480);
            StartPosition = FormStartPosition.CenterScreen;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { /* sin icono */ }

            var top = Flow();
            // Etiqueta y selector de idioma juntos, para que pasen de línea a la vez.
            var langBox = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(16, 0, 0, 0) };
            langBox.Controls.AddRange(new Control[] { lblLang, cbLang });
            top.Controls.AddRange(new Control[] { lblLocation, cbLocation, btnRefresh, btnAddFolder, btnCloudHelp, langBox });
            var actions = Flow();
            actions.Controls.AddRange(new Control[] { btnReplace, btnCopyTo, btnExport, btnRestore, btnConvertFile, btnOpenFolder, btnBackupAll, btnBackups });

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(10) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            root.Controls.Add(top, 0, 0);
            root.Controls.Add(lblHint, 0, 1);
            root.Controls.Add(phases, 0, 2);
            root.Controls.Add(lblCloud, 0, 3);
            root.Controls.Add(lv, 0, 4);
            root.Controls.Add(actions, 0, 5);
            root.Controls.Add(log, 0, 6);
            // Cada fase abre la guía en el paso correspondiente.
            phases.PhaseClicked += i => ShowCloudGuide(i == 0 ? CloudGuideForm.StepTurnOff : i == 1 ? CloudGuideForm.StepManage : CloudGuideForm.StepTurnOn);
            Controls.Add(root);
            mHelp.DropDownItems.AddRange(new ToolStripItem[] { mFaq, mGuide, new ToolStripSeparator(), mKofi, mFollow, new ToolStripSeparator(), mAbout });
            menu.Items.Add(mHelp);
            Controls.Add(menu);
            MainMenuStrip = menu;
            // Redes sociales: submenú «Sígueme» y enlaces en la barra inferior, a la izquierda del Ko-fi.
            foreach (var s in AppInfo.Socials)
            {
                string url = s.Item2;
                mFollow.DropDownItems.Add(s.Item1, null, (o, e) => AppInfo.Open(url));
                var link = new ToolStripStatusLabel { Text = s.Item1, IsLink = true };
                link.Click += (o, e) => AppInfo.Open(url);
                status.Items.Add(link);
            }
            status.Items.Add(kofiLabel);
            Controls.Add(status);
            mFaq.Click += (s, e) => ShowHelp();
            mGuide.Click += (s, e) => ShowCloudGuide();
            mKofi.Click += (s, e) => AppInfo.OpenKofi();
            mAbout.Click += (s, e) => { using (var f = new AboutForm()) f.ShowDialog(this); };
            kofiLabel.Click += (s, e) => AppInfo.OpenKofi();
            EventHandler fitHint = (s, e) =>
            {
                var max = new Size(Math.Max(200, root.ClientSize.Width - root.Padding.Horizontal - 8), 0);
                lblHint.MaximumSize = max;
                lblCloud.MaximumSize = max;
            };
            root.SizeChanged += fitHint;
            Load += fitHint;

            // Entrada 0: idioma del sistema; después, los 5 idiomas por su nombre propio.
            cbLang.SelectedIndexChanged += (s, e) =>
            {
                if (fillingLang || cbLang.SelectedIndex < 0) return;
                I18n.SetChoice(cbLang.SelectedIndex == 0 ? I18n.System : I18n.Languages[cbLang.SelectedIndex - 1].Code);
                ApplyTexts();
            };
            cbLocation.SelectedIndexChanged += (s, e) => RefreshSlots();
            btnRefresh.Click += (s, e) => ReloadLocations();
            btnAddFolder.Click += (s, e) => AddFolder();
            btnCloudHelp.Click += (s, e) => ShowCloudGuide();
            btnReplace.Click += (s, e) => ReplaceFromFile(false);
            btnRestore.Click += (s, e) => ReplaceFromFile(true);
            btnCopyTo.Click += (s, e) => ShowCopyMenu();
            btnExport.Click += (s, e) => ExportSlot();
            btnConvertFile.Click += (s, e) => ConvertLooseFile();
            btnOpenFolder.Click += (s, e) => OpenFolder();
            btnBackups.Click += (s, e) => { Directory.CreateDirectory(BackupDir); Process.Start("explorer.exe", Quote(BackupDir)); };
            btnBackupAll.Click += (s, e) => BackupAll();
            lv.SelectedIndexChanged += (s, e) => UpdateButtons();
            lv.DoubleClick += (s, e) => ReplaceFromFile(false);

            ApplyTexts();
            ReloadLocations();
            Log(I18n.T("log.start", BackupDir));
        }

        // ---------- textos y estado ----------

        void ApplyTexts()
        {
            Text = I18n.T("app.title");
            lblLocation.Text = I18n.T("lbl.location");
            lblLang.Text = I18n.T("lbl.language");
            btnRefresh.Text = I18n.T("btn.refresh");
            btnAddFolder.Text = I18n.T("btn.addFolder");
            btnCloudHelp.Text = I18n.T("btn.cloudHelp");
            mHelp.Text = I18n.T("menu.help");
            mFaq.Text = I18n.T("menu.faq");
            mGuide.Text = I18n.T("menu.cloudGuide");
            mKofi.Text = I18n.T("menu.kofi");
            mFollow.Text = I18n.T("menu.follow");
            mAbout.Text = I18n.T("menu.about");
            kofiLabel.Text = I18n.T("kofi.status");
            btnReplace.Text = I18n.T("btn.replace");
            btnCopyTo.Text = I18n.T("btn.copyTo");
            btnExport.Text = I18n.T("btn.export");
            btnRestore.Text = I18n.T("btn.restore");
            btnConvertFile.Text = I18n.T("btn.convertFile");
            btnOpenFolder.Text = I18n.T("btn.openFolder");
            btnBackups.Text = I18n.T("btn.backups");
            btnBackupAll.Text = I18n.T("btn.backupAll");
            fillingLang = true;
            cbLang.BeginUpdate();
            cbLang.Items.Clear();
            cbLang.Items.Add(I18n.T("lang.system", I18n.NameOf(I18n.SystemLang)));
            foreach (var l in I18n.Languages) cbLang.Items.Add(l.Name);
            cbLang.SelectedIndex = I18n.Choice == I18n.System ? 0 : 1 + Array.FindIndex(I18n.Languages, l => l.Code == I18n.Choice);
            cbLang.EndUpdate();
            fillingLang = false;
            lv.Columns.Clear();
            lv.Columns.Add(I18n.T("col.game"), 110);
            lv.Columns.Add(I18n.T("col.slot"), 60);
            lv.Columns.Add(I18n.T("col.status"), 220);
            lv.Columns.Add(I18n.T("col.ach"), 100);
            lv.Columns.Add(I18n.T("col.deadgod"), 75);
            lv.Columns.Add(I18n.T("col.modified"), 140);
            lv.Columns.Add(I18n.T("col.file"), 240);
            // Volver a poner los elementos para que el combo muestre el texto en el idioma nuevo.
            int sel = cbLocation.SelectedIndex;
            var items = cbLocation.Items.Cast<object>().ToArray();
            cbLocation.BeginUpdate();
            cbLocation.Items.Clear();
            cbLocation.Items.AddRange(items);
            cbLocation.SelectedIndex = Math.Min(sel, items.Length - 1);
            cbLocation.EndUpdate();
            RefreshSlots();
        }

        void ReloadLocations()
        {
            string selected = (cbLocation.SelectedItem as SaveLocation)?.Id;
            var all = Locations.Find();
            all.AddRange(customLocations);
            cbLocation.BeginUpdate();
            cbLocation.Items.Clear();
            foreach (var l in all) cbLocation.Items.Add(l);
            int idx = all.FindIndex(l => l.Id == selected);
            cbLocation.SelectedIndex = idx >= 0 ? idx : 0;
            cbLocation.EndUpdate();
            RefreshSlots();
        }

        SaveLocation CurrentLocation => cbLocation.SelectedItem as SaveLocation;

        void RefreshSlots()
        {
            var loc = CurrentLocation;
            var keep = Selected();
            lv.BeginUpdate();
            lv.Items.Clear();
            if (loc != null)
            {
                lblHint.Text = HintFor(loc);
                ShowCloudState(loc);
                foreach (var v in Versions)
                    for (int slot = 1; slot <= 3; slot++)
                    {
                        var row = ReadRow(loc, v, slot);
                        var item = new ListViewItem(new[]
                        {
                            I18n.VersionName(v), slot.ToString(), StatusText(row),
                            row.File == null ? "" : AchText(row.File.Summarize()),
                            row.File == null ? "" : I18n.T(row.File.Summarize().DeadGod ? "yes" : "no"),
                            row.Exists ? System.IO.File.GetLastWriteTime(row.Path).ToString("g") : "",
                            SaveLocation.FileName(v, slot),
                        }) { Tag = row };
                        if (row.Error != null) item.ForeColor = Color.Firebrick;
                        else if (!row.Exists) item.ForeColor = SystemColors.GrayText;
                        lv.Items.Add(item);
                        if (keep != null && keep.Version == v && keep.Slot == slot) item.Selected = true;
                    }
            }
            lv.EndUpdate();
            UpdateButtons();
        }

        static SlotRow ReadRow(SaveLocation loc, GameVersion v, int slot)
        {
            var row = new SlotRow { Location = loc, Version = v, Slot = slot, Path = loc.PathFor(v, slot) };
            row.Exists = System.IO.File.Exists(row.Path);
            if (!row.Exists) return row;
            try { row.File = SaveFile.Load(row.Path); }
            catch (SaveFormatException ex) { row.Error = I18n.T(ex.Key); }
            catch (Exception ex) { row.Error = I18n.T("err.read", ex.Message); }
            return row;
        }

        static string StatusText(SlotRow row)
        {
            if (!row.Exists) return I18n.T("status.missing");
            if (row.Error != null) return row.Error;
            if (row.File.Version != row.Version) return I18n.T("status.wrongVersion", I18n.VersionName(row.File.Version));
            if (row.Version == GameVersion.RepentancePlus && row.File.Block(SaveFile.AchievementsBlock).Count < SaveFile.RepPlusAchievements)
                return I18n.T("status.olderPlus");
            return I18n.T("status.ok");
        }

        static string HintFor(SaveLocation loc)
        {
            Func<GameVersion, string> cloud = v =>
            {
                var c = Locations.CloudEnabled(v);
                return I18n.T(c == null ? "cloud.unknown" : c.Value ? "cloud.on" : "cloud.off");
            };
            switch (loc.Kind)
            {
                case LocationKind.Steam: return I18n.T("hint.steam", cloud(GameVersion.RepentancePlus), cloud(GameVersion.Repentance));
                case LocationKind.Documents: return I18n.T("hint.local", cloud(GameVersion.RepentancePlus), cloud(GameVersion.Repentance));
                default: return I18n.T("hint.custom");
            }
        }

        void ShowCloudState(SaveLocation loc)
        {
            // Variable local: Control.Visible devuelve false mientras la ventana aún no se muestra.
            bool steam = loc.Kind == LocationKind.Steam;
            lblCloud.Visible = steam;
            phases.Visible = steam;
            if (!steam) return;
            var on = Locations.SteamCloudForIsaac(loc);
            bool wrote = writtenThisSession.Contains(loc.Id);
            // Fase actual: 0 apagar, 1 gestionar, 2 encender; 3 = ciclo terminado (se escribió y la nube vuelve a estar encendida).
            phases.Current = on == false ? (wrote ? 2 : 1) : (wrote ? 3 : 0);
            tips.SetToolTip(phases, I18n.T("phase.tip"));
            lblCloud.Text = (on == false ? "✔ " : "⚠ ") + I18n.T(on == null ? "cloud.isaacUnknown" : on.Value ? "cloud.isaacOn" : "cloud.isaacOff");
            lblCloud.ForeColor = on == false ? Color.ForestGreen : Color.Firebrick;
        }

        void ShowHelp(int question = 1)
        {
            using (var f = new HelpForm(question))
            {
                f.OpenGuideRequested += () => ShowCloudGuide();
                f.ShowDialog(this);
            }
        }

        void ShowCloudGuide(int startStep = CloudGuideForm.StepTurnOff, bool afterWrite = false)
        {
            using (var f = new CloudGuideForm(startStep, afterWrite)) f.ShowDialog(this);
            RefreshSlots(); // el usuario puede haber cambiado Steam Cloud mientras leía la guía
        }

        // Steam Cloud se desactiva a mano: con él activado, Steam volvería a poner el guardado de la nube.
        bool CloudAllowsWriting(SaveLocation loc)
        {
            if (loc.Kind != LocationKind.Steam) return true;
            var on = Locations.SteamCloudForIsaac(loc);
            if (on == false) return true;
            if (on == true)
            {
                if (Ask(I18n.T("cloud.blocked"), I18n.T("title.warn"), MessageBoxIcon.Warning) == DialogResult.Yes) ShowCloudGuide();
                return false;
            }
            if (Ask(I18n.T("cloud.unknownAsk"), I18n.T("title.warn"), MessageBoxIcon.Warning) == DialogResult.Yes) return true;
            ShowCloudGuide();
            return false;
        }

        static string AchText(SaveSummary s) => s.Achievements + "/" + s.AchievementsTotal;

        static string SummaryText(SaveFile f)
        {
            var s = f.Summarize();
            return I18n.T("summary.fmt", I18n.VersionName(f.Version), s.Achievements, s.AchievementsTotal, s.DeadGod ? " · Dead God" : "");
        }

        static string SlotLabel(SlotRow r) => I18n.T("slot.label", I18n.VersionName(r.Version), r.Slot);

        SlotRow Selected() => lv.SelectedItems.Count > 0 ? (SlotRow)lv.SelectedItems[0].Tag : null;

        void UpdateButtons()
        {
            var r = Selected();
            btnReplace.Enabled = btnRestore.Enabled = btnOpenFolder.Enabled = r != null;
            btnCopyTo.Enabled = btnExport.Enabled = r?.File != null;
        }

        // ---------- acciones ----------

        void ReplaceFromFile(bool fromBackup)
        {
            var row = Selected();
            if (row == null) { Info(I18n.T("msg.selectSlot")); return; }
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = I18n.T(fromBackup ? "dlg.restore" : "dlg.openSave");
                dlg.Filter = fromBackup ? I18n.T("filter.backup", SaveLocation.FileName(row.Version, row.Slot)) : I18n.T("filter.dat");
                if (fromBackup) { Directory.CreateDirectory(BackupDir); dlg.InitialDirectory = BackupDir; }
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var src = LoadOrReport(dlg.FileName);
                if (src != null) WriteSlot(row, src, dlg.FileName);
            }
        }

        void ShowCopyMenu()
        {
            var row = Selected();
            if (row?.File == null) { Info(I18n.T("msg.slotEmpty")); return; }
            copyMenu.Items.Clear();
            foreach (ListViewItem it in lv.Items)
            {
                var target = (SlotRow)it.Tag;
                if (target == row) continue;
                string label = SlotLabel(target) + "  —  " + (target.File != null ? SummaryText(target.File)
                    : target.Exists ? I18n.T("summary.unreadable") : I18n.T("summary.empty"));
                copyMenu.Items.Add(label, null, (s, e) => WriteSlot(target, row.File, SlotLabel(row) + " (" + row.Path + ")"));
            }
            copyMenu.Show(btnCopyTo, new Point(0, btnCopyTo.Height));
        }

        void ExportSlot()
        {
            var row = Selected();
            if (row?.File == null) { Info(I18n.T("msg.slotEmpty")); return; }
            ExportFile(row.File, SaveLocation.FileName(row.Version, row.Slot), row.Version);
        }

        void ConvertLooseFile()
        {
            using (var dlg = new OpenFileDialog { Title = I18n.T("dlg.openSave"), Filter = I18n.T("filter.dat") })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var src = LoadOrReport(dlg.FileName);
                if (src == null) return;
                var other = src.Version == GameVersion.RepentancePlus ? GameVersion.Repentance : GameVersion.RepentancePlus;
                string from = src.Version == GameVersion.RepentancePlus ? "rep+" : "rep_";
                string to = other == GameVersion.RepentancePlus ? "rep+" : "rep_";
                ExportFile(src, Path.GetFileName(dlg.FileName).Replace(from, to), other);
            }
        }

        void ExportFile(SaveFile src, string suggestedName, GameVersion defaultVersion)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = I18n.T("dlg.export");
                dlg.Filter = I18n.T("filter.export");
                dlg.FilterIndex = defaultVersion == GameVersion.RepentancePlus ? 1 : 2;
                dlg.FileName = suggestedName;
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var target = dlg.FilterIndex == 2 ? GameVersion.Repentance : GameVersion.RepentancePlus;
                var data = ConvertWithConfirm(src, target);
                if (data == null) return;
                try
                {
                    System.IO.File.WriteAllBytes(dlg.FileName, data.Raw);
                    if (data != src) Log(I18n.T("log.converted", I18n.VersionName(src.Version), I18n.VersionName(target)));
                    Log(I18n.T("log.exported", dlg.FileName));
                }
                catch (Exception ex) { Fail(I18n.T("err.io", ex.Message)); }
            }
        }

        void WriteSlot(SlotRow row, SaveFile src, string origin)
        {
            if (SaveWriter.GameRunning()) { Fail(I18n.T("err.gameRunning")); return; }
            if (!CloudAllowsWriting(row.Location)) { RefreshSlots(); return; }
            if (row.Location.Kind == LocationKind.Steam && SaveWriter.SteamRunning()
                && Ask(I18n.T("warn.steamRunning"), I18n.T("title.warn"), MessageBoxIcon.Warning) != DialogResult.Yes) return;
            var data = ConvertWithConfirm(src, row.Version);
            if (data == null) return;
            string current = row.File != null ? SummaryText(row.File) : row.Exists ? I18n.T("summary.unreadable") : I18n.T("summary.empty");
            if (Ask(I18n.T("confirm.replace", SlotLabel(row), current, SummaryText(src), origin), I18n.T("title.confirm"), MessageBoxIcon.Question) != DialogResult.Yes) return;

            string dir = Path.GetDirectoryName(row.Path);
            bool remindCloud = false;
            try
            {
                string backup = SaveWriter.Write(row.Path, data, BackupDir, row.Location.Id);
                if (backup != null) Log(I18n.T("log.backup", backup));
                if (data != src) Log(I18n.T("log.converted", I18n.VersionName(src.Version), I18n.VersionName(row.Version)));
                Log(I18n.T("log.written", row.Path));
                if (Directory.GetFiles(dir, "rgon_steam_persistentgamedata*.dat").Length > 0) Log(I18n.T("log.rgon"));
                if (row.Location.Kind == LocationKind.Steam)
                {
                    Log(I18n.T("log.cloud"));
                    writtenThisSession.Add(row.Location.Id);
                    remindCloud = true;
                }
            }
            catch (UnauthorizedAccessException) { Fail(I18n.T("err.access", dir)); }
            catch (SaveFormatException ex) { Fail(I18n.T(ex.Key)); }
            catch (Exception ex) { Fail(I18n.T("err.io", ex.Message)); }
            RefreshSlots();
            // Tras escribir en Steam, la guía se abre directamente en «vuelve a encender Steam Cloud antes de jugar».
            if (remindCloud) ShowCloudGuide(CloudGuideForm.StepTurnOn, true);
        }

        // Convierte si hace falta y pregunta antes si se pierden datos. Devuelve null si el usuario cancela.
        SaveFile ConvertWithConfirm(SaveFile src, GameVersion target)
        {
            ConversionLoss loss;
            var data = src.ConvertTo(target, out loss);
            if (loss.Any && Ask(I18n.T("warn.loss", loss.Achievements, loss.Counters), I18n.T("title.warn"), MessageBoxIcon.Warning) != DialogResult.Yes)
                return null;
            return data;
        }

        SaveFile LoadOrReport(string path)
        {
            try { return SaveFile.Load(path); }
            catch (SaveFormatException ex) { Fail(I18n.T(ex.Key)); }
            catch (Exception ex) { Fail(I18n.T("err.read", ex.Message)); }
            return null;
        }

        // Copia manual de todas las ranuras de la ubicación elegida. Solo lee los guardados, así que no exige
        // cerrar el juego ni apagar Steam Cloud. Usa los mismos nombres que las copias automáticas,
        // para que «Restaurar copia…» las encuentre.
        void BackupAll()
        {
            var loc = CurrentLocation;
            if (loc == null) return;
            int n = 0;
            try
            {
                foreach (ListViewItem it in lv.Items)
                {
                    var row = (SlotRow)it.Tag;
                    if (!System.IO.File.Exists(row.Path)) continue;
                    Log(I18n.T("log.backup", SaveWriter.Backup(row.Path, BackupDir, loc.Id)));
                    n++;
                }
            }
            catch (Exception ex) { Fail(I18n.T("err.io", ex.Message)); return; }
            if (n == 0) Info(I18n.T("msg.backupAllNone"));
            else Info(I18n.T("msg.backupAllDone", n, BackupDir));
        }

        void AddFolder()
        {
            using (var dlg = new FolderBrowserDialog { Description = I18n.T("dlg.folder") })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var loc = Locations.Custom(dlg.SelectedPath);
                if (customLocations.All(l => l.Id != loc.Id)) customLocations.Add(loc);
                ReloadLocations();
                cbLocation.SelectedIndex = cbLocation.Items.Cast<SaveLocation>().ToList().FindIndex(l => l.Id == loc.Id);
            }
        }

        void OpenFolder()
        {
            var row = Selected();
            var loc = CurrentLocation;
            if (loc == null) return;
            string dir = row != null ? Path.GetDirectoryName(row.Path) : loc.FolderFor(GameVersion.RepentancePlus);
            if (row != null && row.Exists) Process.Start("explorer.exe", "/select," + Quote(row.Path));
            else if (Directory.Exists(dir)) Process.Start("explorer.exe", Quote(dir));
        }

        // ---------- utilidades ----------

        // Portable: las copias van junto al .exe; si esa carpeta no admite escritura, a %LOCALAPPDATA%.
        static string backupDir;
        static string BackupDir
        {
            get
            {
                if (backupDir != null) return backupDir;
                string exeDir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "backups");
                try
                {
                    Directory.CreateDirectory(exeDir);
                    string probe = Path.Combine(exeDir, ".probe");
                    System.IO.File.WriteAllText(probe, "");
                    System.IO.File.Delete(probe);
                    return backupDir = exeDir;
                }
                catch
                {
                    return backupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IsaacSaveManager", "backups");
                }
            }
        }

        void Log(string msg) => log.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + Environment.NewLine);

        void Fail(string msg)
        {
            Log(I18n.T("log.error", msg.Replace("\n", " ")));
            MessageBox.Show(this, msg, I18n.T("title.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void Info(string msg) => MessageBox.Show(this, msg, I18n.T("app.title"), MessageBoxButtons.OK, MessageBoxIcon.Information);

        DialogResult Ask(string msg, string title, MessageBoxIcon icon) =>
            MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo, icon, MessageBoxDefaultButton.Button2);

        static string Quote(string p) => "\"" + p + "\"";

        static Label AutoLabel() => new Label { AutoSize = true, Margin = new Padding(3, 7, 3, 3) };
        static Button Btn() => new Button { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(6, 2, 6, 2) };
        static FlowLayoutPanel Flow() => new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
    }
}
