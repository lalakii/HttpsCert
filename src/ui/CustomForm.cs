using httpscert.src.lang;
using httpscert.src.util;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace httpscert.src.ui
{
    internal sealed class CustomForm : LaUIForm.ILaUI, IDisposable
    {
        private static readonly bool CN = LaUIForm.IsChinese;
        private readonly Button btn = new() { BackColor = Color.GhostWhite, AutoSize = true, Text = CN ? LangRes.GenCN : LangRes.Gen, TextAlign = ContentAlignment.MiddleCenter };
        private readonly ComboBox cbx = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox domains = new() { Multiline = true, Height = 180, ScrollBars = ScrollBars.Vertical };
        private readonly LaUIForm frm;
        private readonly TextBox pass = new() { UseSystemPasswordChar = true };
        private readonly Label tip2 = new() { AutoSize = true, Text = CN ? LangRes.ValidForCN : LangRes.ValidFor, TextAlign = ContentAlignment.MiddleCenter };
        private readonly Label tip3 = new() { Text = CN ? LangRes.PwdCN : LangRes.Pwd, TextAlign = ContentAlignment.MiddleCenter, AutoSize = true, BackColor = Color.GhostWhite };
        private readonly Label tip4 = new() { Text = CN ? LangRes.YearsCN : LangRes.Years, TextAlign = ContentAlignment.MiddleCenter, AutoSize = true, BackColor = Color.GhostWhite };
        private readonly Label tips = new() { AutoSize = true, Left = 12, Top = 17, Text = CN ? LangRes.DomainTipCN : LangRes.DomainTip, TextAlign = ContentAlignment.MiddleCenter };

        public CustomForm()
        {
            using RNGCryptoServiceProvider rnd = new();
            var color = new byte[4];
            rnd.GetBytes(color, 0, 4);
            frm = new(this)
            {
                BorderVisible = true,
                ColorPrimary = Color.FromArgb(color[0], color[1], color[2], color[3]),
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

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("msvcrt.dll", CharSet = CharSet.Auto)]
        public static extern int system(byte[] command);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposed)
        {
            if (disposed)
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
        }

        public void OnClose()
        {
            Application.Exit();
            Environment.Exit(0);
        }

        public void OnHelp()
        {
            if (system(Encoding.Default.GetBytes($"start {LangRes.Url}")) != 0)
            {
                MessageBox.Show(LangRes.Url);
            }
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
            content.Width = tips.Width + (domains.Left * 2);
            domains.Width = content.Width - (domains.Left * 2) - 4;
            cbx.Width = TextRenderer.MeasureText(cbx.Text, cbx.Font).Width * 3;
            cbx.Top = domains.Top + domains.Height + 7;
            tip2.Top = ((cbx.Height - tip2.Height) / 2) + cbx.Top;
            tip2.Left = tip3.Left = 12;
            cbx.Left = tip2.Left + tip2.Width + 5;
            tip4.Left = cbx.Width + cbx.Left + 5;
            tip4.Top = tip2.Top;
            tip3.Top = cbx.Top + tip3.Height + 22;
            pass.Top = tip3.Top + ((tip3.Height - pass.Height) / 2);
            pass.Left = tip3.Left + tip3.Width + 7;
            pass.Width = content.Width - pass.Left - (domains.Left * 2) - btn.Width - 80;
            var btnFontSize = TextRenderer.MeasureText(btn.Text, btn.Font);
            btn.Width = (int)(btnFontSize.Width * 1.2);
            btn.Height = (int)(btnFontSize.Height * 1.6);
            btn.Top = tip3.Top - (btn.Height / 2) - 8;
            btn.Left = content.Width - btn.Width - 20;
            content.Height = pass.Top + pass.Height + (pass.Height / 2);
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
                if (folder.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                List<string> dms = [];
                var rdms = domainValue.Split('\n');
                if (rdms.Length < 1)
                {
                    return;
                }

                var invalidChars = Path.GetInvalidFileNameChars();
                foreach (var it in rdms)
                {
                    var item = it.Replace("\r", string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        foreach (var c in invalidChars)
                        {
                            item = item.Replace(c.ToString(), string.Empty);
                        }

                        dms.Add(item);
                    }
                }

                if (Utils.Generate(folder.SelectedPath, [.. dms], pass.Text, cbx.SelectedIndex + 1) == 0)
                {
                    MessageBox.Show(CN ? LangRes.SuccessCN : LangRes.Success);
                }
            }
        }
    }
}