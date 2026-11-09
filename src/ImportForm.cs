using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace IsaacSaveManager
{
    /// <summary>
    /// Importar un guardado externo: ya elegido el archivo, se elige la ranura de destino (de cualquier ubicación,
    /// por defecto la cuenta de Steam elegida) y se ve si se va a convertir y qué se perdería.
    /// La escritura la hace la ventana principal (copia de seguridad, comprobación de Steam Cloud, etc.).
    /// </summary>
    sealed class ImportForm : Form
    {
        public SlotTarget Selected { get; private set; }

        public sealed class SlotTarget
        {
            public SaveLocation Location;
            public GameVersion Version;
            public int Slot;
            public string Contents;
            public override string ToString() =>
                Location + "  —  " + I18n.T("slot.label", I18n.VersionName(Version), Slot) + "  —  " + Contents;
        }

        readonly SaveFile source;
        readonly ComboBox cbTarget = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 760, DropDownWidth = 760 };
        readonly Label lblConvert = new Label { AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(3, 10, 3, 3) };
        readonly Label lblLoss = new Label { AutoSize = true, ForeColor = Color.Firebrick, Margin = new Padding(3, 4, 3, 3) };
        readonly Button btnOk = new Button { AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Padding = new Padding(8, 2, 8, 2) };

        public ImportForm(SaveFile source, string sourcePath, string sourceSummary, IEnumerable<SlotTarget> targets, SlotTarget preselect)
        {
            this.source = source;
            Font = new Font("Segoe UI", 9.5F);
            AutoScaleMode = AutoScaleMode.Dpi;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Text = I18n.T("imp.title");
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { /* sin icono */ }

            var bold = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            var root = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Padding = new Padding(16) };
            root.Controls.Add(new Label { AutoSize = true, Text = I18n.T("imp.source"), Font = bold });
            root.Controls.Add(new Label { AutoSize = true, Text = sourcePath, MaximumSize = new Size(760, 0), ForeColor = SystemColors.GrayText });
            root.Controls.Add(new Label { AutoSize = true, Text = sourceSummary, Margin = new Padding(3, 2, 3, 12) });
            root.Controls.Add(new Label { AutoSize = true, Text = I18n.T("imp.target"), Font = bold });
            foreach (var t in targets) cbTarget.Items.Add(t);
            root.Controls.Add(cbTarget);
            root.Controls.Add(lblConvert);
            root.Controls.Add(lblLoss);
            root.Controls.Add(new Label { AutoSize = true, Text = I18n.T("imp.hint"), MaximumSize = new Size(760, 0), ForeColor = SystemColors.GrayText, Margin = new Padding(3, 10, 3, 10) });

            var cancel = new Button { AutoSize = true, Text = I18n.T("imp.cancel"), DialogResult = DialogResult.Cancel };
            btnOk.DialogResult = DialogResult.OK;
            var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            buttons.Controls.AddRange(new Control[] { cancel, btnOk });
            root.Controls.Add(buttons);
            Controls.Add(root);
            AcceptButton = btnOk;
            CancelButton = cancel;

            cbTarget.SelectedIndexChanged += (s, e) => UpdateInfo();
            int idx = preselect == null ? 0 : cbTarget.Items.Cast<SlotTarget>().ToList()
                .FindIndex(t => t.Location.Id == preselect.Location.Id && t.Version == preselect.Version && t.Slot == preselect.Slot);
            if (cbTarget.Items.Count > 0) cbTarget.SelectedIndex = Math.Max(0, idx);
            FormClosing += (s, e) => { if (DialogResult == DialogResult.OK) Selected = cbTarget.SelectedItem as SlotTarget; };
        }

        void UpdateInfo()
        {
            var t = cbTarget.SelectedItem as SlotTarget;
            if (t == null) return;
            bool convert = source.Version != t.Version;
            lblConvert.Text = convert
                ? "⇄ " + I18n.T("imp.convertYes", I18n.VersionName(source.Version), I18n.VersionName(t.Version))
                : "✔ " + I18n.T("imp.convertNo", I18n.VersionName(source.Version));
            lblConvert.ForeColor = convert ? Color.FromArgb(200, 110, 20) : Color.ForestGreen;
            ConversionLoss loss;
            source.ConvertTo(t.Version, out loss);
            lblLoss.Text = loss.Any ? "⚠ " + I18n.T("imp.loss", loss.Achievements, loss.Counters) : "";
            lblLoss.Visible = loss.Any;
            btnOk.Text = I18n.T(convert ? "imp.okConvert" : "imp.ok");
        }
    }
}
