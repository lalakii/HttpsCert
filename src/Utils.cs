using httpscert.src;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Reflection;
using System.Resources;
using BigInteger = Org.BouncyCastle.Math.BigInteger;

[assembly: NeutralResourcesLanguage("en")]
var mArgs = args ?? [];
string? domain = null, pass = null;
int year = 1;
for (int i = 0; i < mArgs.Length; i++)
{
    var item = mArgs[i].Replace("-", "/");
    if (i + 1 < mArgs.Length)
    {
        if (item.StartsWith("/d", StringComparison.OrdinalIgnoreCase))
        {
            domain = mArgs[i + 1].Replace('=', '-');
        }
        else if (item.StartsWith("/p", StringComparison.OrdinalIgnoreCase))
        {
            pass = mArgs[i + 1];
        }
        else if (item.StartsWith("/y", StringComparison.OrdinalIgnoreCase) && !int.TryParse(mArgs[i + 1], out year))
        {
            year = 1;
        }
    }
}
if (domain == null)
{
    var bc = typeof(BigInteger).Assembly;
    var bcp = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(bc, typeof(AssemblyProductAttribute), false)).Product;
    var bcv = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(bc, typeof(AssemblyFileVersionAttribute), false)).Version;
    Console.WriteLine(LangRes.Load, bcp, bcv);
    Console.Write(LangRes.Title.Replace("\\\\t", "\t\t"));
    return;
}
if (year < 1 || year > 5000)
{
    year = 100;
}
var alias = $"CN={domain}";
SecureRandom rand = new();
RsaKeyPairGenerator gen = new();
gen.Init(new(rand, 2048));
var key = gen.GenerateKeyPair();
WriteObject($"{domain}.key", key.Private);//key
GeneralName dns = new(GeneralName.DnsName, domain);
X509Name name = new(alias);
X509V3CertificateGenerator x509 = new();
x509.SetIssuerDN(name);
x509.SetSubjectDN(name);
var date = DateTime.Now;
x509.SetNotBefore(date);
x509.SetNotAfter(date.AddYears(year));
x509.SetPublicKey(key.Public);
x509.SetSerialNumber(BigInteger.ValueOf(rand.NextLong()).Abs());
x509.AddExtension(X509Extensions.SubjectAlternativeName, false, new GeneralNames([dns, dns]));
var crt = x509.Generate(new Asn1SignatureFactory(PkcsObjectIdentifiers.Sha256WithRsaEncryption.Id, key.Private, rand));
WriteObject($"{domain}.crt", crt);//crt
if (pass != null)
{
    using var fs = File.Create($"{domain}.pfx");//pfx
    var build = new Pkcs12StoreBuilder().Build();
    build.SetKeyEntry(domain, new(key.Private), [new(crt)]);
    build.Save(fs, pass.ToCharArray(), rand);
}
Console.WriteLine(LangRes.Success);
static void WriteObject(string fileName, object obj)
{
    using var fs = File.Create(fileName);
    using StreamWriter sw = new(fs);
    using PemWriter pw = new(sw);
    pw.WriteObject(obj);
}