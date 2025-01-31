using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace httpscert.src
{
    internal sealed class CustomForm : LaUIForm.ILaUI, IDisposable
    {
        private static readonly bool CN = LaUIForm.IsChinese;
        private readonly Button btn = new() { AutoSize = true, Text = CN ? LangRes.GenCN : LangRes.Gen, TextAlign = ContentAlignment.MiddleCenter };
        private readonly ComboBox cbx = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 64 };
        private readonly TextBox domains = new() { Multiline = true, Width = 380, Height = 180, ScrollBars = ScrollBars.Vertical }, pass = new() { UseSystemPasswordChar = true };
        private readonly LaUIForm frm;
        private readonly Label tips = new() { AutoSize = true, Left = 12, Top = 12, Text = CN ? LangRes.DomainTipCN : LangRes.DomainTip, TextAlign = ContentAlignment.MiddleCenter }, tip2 = new() { AutoSize = true, Text = CN ? LangRes.ValidForCN : LangRes.ValidFor, TextAlign = ContentAlignment.MiddleCenter }, tip3 = new() { Text = CN ? LangRes.PwdCN : LangRes.Pwd, TextAlign = ContentAlignment.MiddleCenter, AutoSize = true, BackColor = Color.GhostWhite }, tip4 = new() { Text = CN ? LangRes.YearsCN : LangRes.Years, TextAlign = ContentAlignment.MiddleCenter, AutoSize = true, BackColor = Color.GhostWhite };

        public CustomForm()
        {
            using var rnd = RandomNumberGenerator.Create();
            byte[] color = new byte[3];
            rnd.GetBytes(color, 0, 1);
            rnd.GetBytes(color, 1, 1);
            rnd.GetBytes(color, 2, 1);
            frm = new(this)
            {
                BorderVisible = true,
                ColorPrimary = Color.FromArgb(color[0], color[1], color[2]),
                WindowTitle = CN ? LangRes.WindowTitleCN : LangRes.WindowTitle,
                BorderColor = Color.LightGray,
            };
            for (int i = 1; i < 101; i++)
            {
                cbx.Items.Add(i);
            }
            cbx.SelectedIndex = 0;
            btn.Click += Btn_Click;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool _)
        {
            if (!pass.IsDisposed)
            {
                pass.Dispose();
            }
            if (!cbx.IsDisposed)
            {
                cbx.Dispose();
            }
            if (!btn.IsDisposed)
            {
                btn.Dispose();
            }
            if (!domains.IsDisposed)
            {
                domains.Dispose();
            }
            if (!tip4.IsDisposed)
            {
                tip4.Dispose();
            }
            if (!tip3.IsDisposed)
            {
                tip3.Dispose();
            }
            if (!tip2.IsDisposed)
            {
                tip2.Dispose();
            }
            if (!tips.IsDisposed)
            {
                tips.Dispose();
            }
            if (!frm.IsDisposed)
            {
                frm.Dispose();
            }
        }

        public void OnClose()
        {
            Application.Exit();
            Environment.Exit(0);
        }

        public void OnHelp()
        {
            Process.Start("http://https.sourceforge.net/");
        }

        public void OnInit(Panel content)
        {
            var c = content.Controls;
            tip2.BackColor = Color.GhostWhite;
            tips.BackColor = Color.GhostWhite;
            c.AddRange([btn, domains, pass, tips, cbx, tip2, tip3, tip4]);
        }

        public void OnLayout(Panel content)
        {
            domains.Top = tips.Top + tips.Height + 4;
            domains.Left = tips.Left;
            content.Width = domains.Width + (domains.Left * 2);
            cbx.Top = domains.Top + domains.Height + 7;
            tip2.Top = cbx.Top + (cbx.Height - tip2.Height);
            tip2.Left = tip3.Left = 12;
            cbx.Left = tip2.Left + tip2.Width + 5;
            tip4.Left = cbx.Width + cbx.Left + 5;
            tip4.Top = tip2.Top;
            tip3.Top = cbx.Top + tip3.Height + 12;
            pass.Top = tip3.Top + (tip3.Height - pass.Height);
            pass.Left = tip3.Left + tip3.Width + 7;
            pass.Width = content.Width - pass.Left - (domains.Left * 2) - btn.Width - 80;
            btn.Height = 42;
            btn.Top = tip3.Top - (btn.Height / 2) - 4;
            btn.Left = content.Width - btn.Width - 20;
            content.Height = pass.Top + pass.Height + 11;
        }

        public void Run()
        {
            frm.Run();
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            var domainValue = domains.Text.Trim();
            if (!string.IsNullOrWhiteSpace(domainValue))
            {
                using FolderBrowserDialog folder = new() { ShowNewFolderButton = true, Description = CN ? LangRes.SaveToCN : LangRes.SaveTo };
                if (DialogResult.OK == folder.ShowDialog())
                {
                    List<string> dms = [];
                    var rdms = domainValue.Split('\n');
                    if (rdms.Length > 0)
                    {
                        var invalidChars = Path.GetInvalidFileNameChars();
                        foreach (var it in rdms)
                        {
                            var item = it.Replace("\r", "").Trim();
                            if (!string.IsNullOrWhiteSpace(item))
                            {
                                foreach (var c in invalidChars)
                                {
                                    item = item.Replace(c.ToString(), string.Empty);
                                }
                                dms.Add(item);
                            }
                        }
                    }
                    Utils.Generate(folder.SelectedPath, [.. dms], pass.Text, cbx.SelectedIndex + 1);
                }
            }
        }
    }
}