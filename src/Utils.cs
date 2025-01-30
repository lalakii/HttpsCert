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
var bc = typeof(BigInteger).Assembly;
var bcv = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(bc, typeof(AssemblyFileVersionAttribute), false)).Version;
var bcn = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(bc, typeof(AssemblyProductAttribute), false)).Product;
Console.WriteLine($"{LangRes.Load}{bcn} - v{bcv}\n");
int domainIndex = -1, passIndex = -1, yearIndex = -1;
for (int i = 0; i < mArgs.Length; i++)
{
    var item = mArgs[i].Replace("-", "/");
    if (item.StartsWith("/d", StringComparison.OrdinalIgnoreCase))
    {
        domainIndex = i;
    }
    else if (item.StartsWith("/p", StringComparison.OrdinalIgnoreCase))
    {
        passIndex = i;
    }
    else if (item.StartsWith("/y", StringComparison.OrdinalIgnoreCase))
    {
        yearIndex = i;
    }
}
if (domainIndex == -1 || domainIndex + 1 > mArgs.Length - 1)
{
    Console.Write(LangRes.Title.Replace("\\\\t", "\t\t"));
    return;
}
int year = 1;
if (yearIndex != -1 && yearIndex + 1 < mArgs.Length && !int.TryParse(mArgs[yearIndex + 1], out year))
{
    year = 1;
}
var domain = mArgs[domainIndex + 1].Replace('=', '-');
var alias = $"CN={domain}";
using StringWriter writer = new();
using PemWriter pw = new(writer);
SecureRandom rand = new();
RsaKeyPairGenerator gen = new();
gen.Init(new(rand, 2048));
var keyPair = gen.GenerateKeyPair();
pw.WriteObject(keyPair.Private);
var mKey = writer.ToString();//key
File.WriteAllText($"{domain}.key", mKey);
X509V3CertificateGenerator x509 = new();
X509Name name = new(alias);
x509.SetIssuerDN(name);
x509.SetSubjectDN(name);
var date = DateTime.Now;
x509.SetNotBefore(date);
x509.SetNotAfter(date.AddYears(year));
x509.SetPublicKey(keyPair.Public);
x509.SetSerialNumber(BigInteger.ValueOf(Math.Abs(rand.NextLong())));
GeneralName dns = new(GeneralName.DnsName, domain);
x509.AddExtension(X509Extensions.SubjectAlternativeName, false, new GeneralNames([dns, dns]));
var cert = x509.Generate(new Asn1SignatureFactory(new AlgorithmIdentifier(PkcsObjectIdentifiers.Sha256WithRsaEncryption), keyPair.Private));
writer.GetStringBuilder().Clear();
pw.WriteObject(cert);
var mCert = writer.ToString();//cert
File.WriteAllText($"{domain}.crt", mCert);
if (passIndex != -1 && passIndex + 1 < mArgs.Length)
{
    using var fs = File.Create($"{domain}.pfx");//pfx
    var build = new Pkcs12StoreBuilder().Build();
    build.SetKeyEntry(domain, new(keyPair.Private), [new(cert)]);
    build.Save(fs, mArgs[passIndex + 1].ToCharArray(), rand);
}
Console.WriteLine(LangRes.Success);