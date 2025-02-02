using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Net;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

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

            RsaKeyPairGenerator rsa = new();
            SecureRandom rand = new();
            rsa.Init(new(rand, 2048));
            var key = rsa.GenerateKeyPair();
            WriteObject(dir, $"{domain}.key", key.Private); // key
            X509V3CertificateGenerator x509 = new();
            var alias = $"CN={domain}, O={domain}, OU={domain}";
            X509Name name = new(alias);
            x509.SetIssuerDN(name);
            x509.SetSubjectDN(name);
            var date = DateTime.Now;
            x509.SetNotBefore(date);
            x509.SetNotAfter(date.AddYears(year));
            x509.SetPublicKey(key.Public);
            var data = new byte[24];
            rand.NextBytes(data);
            x509.SetSerialNumber(new Org.BouncyCastle.Math.BigInteger(data).Abs());
            List<GeneralName> names = [];
            foreach (var it in domains)
            {
                if (it.Contains('@'))
                {
                    names.Add(new(GeneralName.Rfc822Name, it));
                }
                else if (IPAddress.IsValid(it))
                {
                    names.Add(new(GeneralName.IPAddress, it));
                }
                else
                {
                    names.Add(new(GeneralName.DnsName, it));
                }
            }

            x509.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(true));
            x509.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyCertSign | KeyUsage.CrlSign));
            x509.AddExtension(X509Extensions.SubjectAlternativeName, false, new GeneralNames([.. names]));
            x509.AddExtension(X509Extensions.SubjectKeyIdentifier, false, new SubjectKeyIdentifierStructure(key.Public));
            x509.AddExtension(X509Extensions.AuthorityKeyIdentifier, false, new AuthorityKeyIdentifierStructure(key.Public));
            var crt = x509.Generate(new Asn1SignatureFactory(PkcsObjectIdentifiers.Sha256WithRsaEncryption.Id, key.Private, rand));
            WriteObject(dir, $"{domain}.crt", crt); // crt
            if (!string.IsNullOrWhiteSpace(pass))
            {
                using var sw = GetWriter(dir, $"{domain}.pfx"); // pfx
                var build = new Pkcs12StoreBuilder().Build();
                build.SetKeyEntry(domain, new(key.Private), [new(crt)]);
                build.Save(sw.BaseStream, pass.ToCharArray(), rand);
            }

            return 0;
        }

        private static StreamWriter GetWriter(string dir, string fileName)
        {
            return new(Path.Combine(dir, fileName), false);
        }

        private static void WriteObject(string dir, string fileName, object obj)
        {
            using var sw = GetWriter(dir, fileName);
            using PemWriter pw = new(sw);
            pw.WriteObject(obj);
        }
    }
}