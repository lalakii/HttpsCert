using httpscert.src;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Resources;
using BigInteger = Org.BouncyCastle.Math.BigInteger;

[assembly: NeutralResourcesLanguage("en")]
var mArgs = args;
mArgs ??= [];
int domainIndex = -1, passindex = -1;
for (int i = 0; i < mArgs.Length; i++)
{
    var item = mArgs[i].Replace("-", "/");
    if (item.StartsWith("/d", StringComparison.OrdinalIgnoreCase))
    {
        domainIndex = i;
    }
    else if (item.StartsWith("/p", StringComparison.OrdinalIgnoreCase))
    {
        passindex = i;
    }
}
if (domainIndex == -1 || domainIndex + 1 > mArgs.Length - 1)
{
    Console.Write(LangRes.Title);
    return;
}
var domain = mArgs[domainIndex + 1].Replace('=', '-');
var alias = $"CN={domain}";
using StringWriter writer = new();
using PemWriter pw = new(writer);
RsaKeyPairGenerator generator = new();
SecureRandom rand = new();
generator.Init(new KeyGenerationParameters(rand, 2048));
AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();
pw.WriteObject(keyPair.Private);
var mKey = writer.ToString();//key
File.WriteAllText($"{domain}.key", mKey);
X509V3CertificateGenerator x509 = new();
X509Name name = new(alias);
x509.SetIssuerDN(name);
x509.SetSubjectDN(name);
var date = DateTime.Now;
x509.SetNotBefore(date);
x509.SetNotAfter(date.AddYears(1));
x509.SetPublicKey(keyPair.Public);
x509.SetSerialNumber(BigInteger.ValueOf(Math.Abs(rand.NextLong())));
x509.AddExtension(X509Extensions.SubjectAlternativeName, false, new GeneralNames([new(GeneralName.DnsName, domain), new(GeneralName.DnsName, domain)]));
var cert = x509.Generate(new Asn1SignatureFactory(new AlgorithmIdentifier(PkcsObjectIdentifiers.Sha256WithRsaEncryption), keyPair.Private));
writer.GetStringBuilder().Clear();
pw.WriteObject(cert);
var mCert = writer.ToString();//cert
File.WriteAllText($"{domain}.crt", mCert);
if (passindex != -1 && passindex + 1 < mArgs.Length)
{
    var build = new Pkcs12StoreBuilder().Build();
    build.SetKeyEntry("test", new AsymmetricKeyEntry(keyPair.Private), [new X509CertificateEntry(cert)]);
    using var fs = File.Create($"{domain}.pfx");//pfx
    build.Save(fs, mArgs[passindex + 1].ToCharArray(), rand);
}
Console.WriteLine(LangRes.Success);