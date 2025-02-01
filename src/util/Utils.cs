using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Net;
using Org.BouncyCastle.X509;

using BigInteger = Org.BouncyCastle.Math.BigInteger;

namespace httpscert.src.util
{
    internal static class Utils
    {
        internal static int Generate(string dir, string[] domains, string pass, int year)
        {
            if (!Directory.Exists(dir))
            {
                return -2;
            }

            if (year < 1 || year > 5000)
            {
                year = 100;
            }

            string domain;
            if (domains.Length > 0)
            {
                domain = domains[0];
            }
            else
            {
                return -1;
            }

            var alias = $"CN={domain}";
            SecureRandom rand = new();
            RsaKeyPairGenerator gen = new();
            gen.Init(new(rand, 2048));
            var key = gen.GenerateKeyPair();
            WriteObject(dir, $"{domain}.key", key.Private); // key
            X509Name name = new(alias);
            X509V3CertificateGenerator x509 = new();
            x509.SetIssuerDN(name);
            x509.SetSubjectDN(name);
            var date = DateTime.Now;
            x509.SetNotBefore(date);
            x509.SetNotAfter(date.AddYears(year));
            x509.SetPublicKey(key.Public);
            x509.SetSerialNumber(BigInteger.ValueOf(rand.NextLong()).Abs());
            List<GeneralName> names = [];
            foreach (var it in domains)
            {
                if (IPAddress.IsValid(it))
                {
                    names.Add(new(GeneralName.IPAddress, it));
                }
                else
                {
                    names.Add(new(GeneralName.DnsName, it));
                }
            }

            x509.AddExtension(X509Extensions.SubjectAlternativeName, false, new GeneralNames([.. names]));
            var crt = x509.Generate(new Asn1SignatureFactory(PkcsObjectIdentifiers.Sha256WithRsaEncryption.Id, key.Private, rand));
            WriteObject(dir, $"{domain}.crt", crt); // crt
            if (!string.IsNullOrWhiteSpace(pass))
            {
                using var fs = File.Create(Path.Combine(dir, $"{domain}.pfx")); // pfx
                var build = new Pkcs12StoreBuilder().Build();
                build.SetKeyEntry(domain, new(key.Private), [new(crt)]);
                build.Save(fs, pass.ToCharArray(), rand);
            }

            return 0;
        }

        private static void WriteObject(string dir, string fileName, object obj)
        {
            using var fs = File.Create(Path.Combine(dir, fileName));
            using StreamWriter sw = new(fs);
            using PemWriter pw = new(sw);
            pw.WriteObject(obj);
        }
    }
}