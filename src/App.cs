using httpscert.src;
using System.IO.Compression;
using System.Reflection;

[assembly: System.Resources.NeutralResourcesLanguage("en")]
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
using MemoryStream dll = new();
using GZipStream gz = new(new MemoryStream(LangRes.bc), CompressionMode.Decompress);
ThreadPool.UnsafeQueueUserWorkItem(_ => gz.CopyToAsync(dll), 0);
AppDomain.CurrentDomain.AssemblyResolve += (_, _) => Assembly.Load(dll.ToArray());
using CustomForm frm = new();
frm.Run();