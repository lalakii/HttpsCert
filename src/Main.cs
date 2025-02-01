using httpscert.src.lang;
using httpscert.src.legacy;
using httpscert.src.ui;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

[assembly: System.Resources.NeutralResourcesLanguage("en")]
using AutoResetEvent evt = new(false);
ThreadPool.UnsafeQueueUserWorkItem(
    _ =>
{
    using MemoryStream gzs = new(LangRes.bc);
    using MemoryStream dll = new();
    using GZipStream gz = new(gzs, CompressionMode.Decompress);
    gz.CopyTo(dll);
    AppDomain.CurrentDomain.AssemblyResolve += (_, _) => System.Reflection.Assembly.Load(dll.ToArray());
    evt.Set();
},
    0);
if (args?.Length > 0 && AllocConsole())
{
    evt.WaitOne();
    if (Legacy.Command(args) != 0)
    {
        Console.WriteLine(LangRes.Title.Replace("\\\\t", "\t"));
    }
    else
    {
        Console.WriteLine(LangRes.Success);
    }

    if (CustomForm.system(Encoding.Default.GetBytes("pause")) != 0)
    {
        Console.ReadKey();
    }
}
else
{
    Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
    Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    using CustomForm frm = new();
    frm.Run();
}

[DllImport("kernel32.dll")]
static extern bool AllocConsole();