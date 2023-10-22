using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace Wasenshi.HemoDialysisPro.Share
{
    public static class LicenseManager
    {
        public const string SERVERKEY = "server.key";
        public const string LICENSEFILE = "license.hmp";

        public static bool ExpiredMode => isExpired;
        public static bool AllowExpireMode => allowExpired;
        public static int MaxUnits => maxUnits;
        public static bool UseLicenseServer => useLicenseServer; // only used for extra validation, not for replacing existing validation
        public static Feature FeatureList => featureList;

        private static bool isExpired = false;
        private static bool allowExpired = false;
        private static int maxUnits = 3;
        private static bool useLicenseServer = false;
        private static Feature featureList;

        private static ILogger logger;
        public static void SetLogger(ILogger logger)
        {
            LicenseManager.logger = logger;
        }

        public static void CheckLicense()
        {
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
#if DEBUG || TEST
            isExpired = config.GetValue<bool>("expiredMode");
            allowExpired = config.GetValue<bool>("allowExpired");
            maxUnits = config.GetValue("maxUnits", 3);
            var isFull = config.GetValue<bool>("isFull");
            featureList = isFull ? Feature.Full : Feature.Management;
            FeatureFlag.SetupFeatureFlags(featureList);
            goto end;
#endif
            // TODO: Add license server checking logic here, using license token for particular user and bypass general license checking below

            // This file trade readbility for obfuscation purpose.
            var rand = RandomNumberGenerator.GetInt32(3);
            bool valid = false;
            switch (rand)
            {
                case 0:
                    valid = HahaCheck2.Check();
                    break;
                case 1:
                    valid = YouCheck3.CheckAny();
                    break;
                case 2:
                    valid = RandomCheck1.Check();
                    break;
            }

            if (!valid)
            {
                logger?.LogError("Modified code! you are great! You should come and work with me. I will pay you more than they pay you ;)");
                isExpired = true;
                FeatureFlag.SetupFeatureFlags(Feature.None);
                goto end;
            }

            if (!File.Exists(SERVERKEY))
            {
                logger?.LogError("server.key is not found.");
                isExpired = true;
                FeatureFlag.SetupFeatureFlags(Feature.None);
                goto end;
            }
            // check server key
            var fingerprint = GetThumbprint(SERVERKEY);
            if (fingerprint != LicenseFingerprint.FINGERPRINT)
            {
                logger?.LogError("server.key is invalid.");
                isExpired = true;
                FeatureFlag.SetupFeatureFlags(Feature.None);
                goto end;
            }

            if (!File.Exists(LICENSEFILE))
            {
                logger?.LogError("license is not found.");
                isExpired = true;
                FeatureFlag.SetupFeatureFlags(Feature.None);
                goto end;
            }

            var data = File.ReadAllBytes(LICENSEFILE);
            var body = data.Skip(256).ToArray();
            var signature = data.Take(256).ToArray();

            RSACryptoServiceProvider csp = GetEncryptor();

            var verified = csp.VerifyData(body, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            if (verified)
            {
                var license = JsonSerializer.Deserialize<License>(body);
                logger?.LogInformation("License Expire Date: {ExpireDate}", license.Expire);
                isExpired = DateTime.UtcNow >= license.Expire;
                allowExpired = license.AllowExpireMode;
                maxUnits = license.MaxUnits;
                logger?.LogInformation("Max Units: {Max}", MaxUnits);
                FeatureFlag.SetupFeatureFlags(license.Features);
                featureList = license.Features;
            }
            else
            {
                isExpired = true;
                FeatureFlag.SetupFeatureFlags(Feature.None);
            }

            rand = RandomNumberGenerator.GetInt32(3);
            switch (rand)
            {
                case 0:
                    valid = YouCheck3.Check();
                    break;
                case 1:
                    valid = HahaCheck2.CheckYo();
                    break;
                case 2:
                    valid = RandomCheck1.CheckWhat();
                    break;
            }

            if (!valid)
            {
                logger?.LogError("Modified code! you are great! You should come and work with me. I will pay you more than they pay you ;)");
                isExpired = true;
                FeatureFlag.SetupFeatureFlags(Feature.None);
                goto end;
            }

        end:
            logger?.LogInformation(isExpired ? FeatureFlag.IsDisabled() ? "License is invalid/expired, block usage." : "License is expired, use expired mode."
                : "License is valid.");
        }

        public static RSACryptoServiceProvider GetEncryptor()
        {
            var csp = new RSACryptoServiceProvider(2048);
            RSAParameters key = GetKey(SERVERKEY);
            csp.ImportParameters(key);

            return csp;
        }

        public static string GetThumbprint(string keyPath)
        {
            using (var fs = File.OpenRead(keyPath))
            {
                var hashed = SHA256.Create().ComputeHash(fs);
                return Convert.ToBase64String(hashed);
            }
        }

        public static RSAParameters GetKey(string keyPath)
        {
            using (var fs = File.OpenRead(keyPath))
            {
                using (var cs = new CryptoStream(fs, new FromBase64Transform(), CryptoStreamMode.Read))
                {
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                    var key = (RSAParameters)xs.Deserialize(cs);
                    return key;
                }
            }
        }

        private class License
        {
            public DateTime Expire { get; set; }
            public Feature Features { get; set; }
            public int MaxUnits { get; set; } = 3;
            public bool AllowExpireMode { get; set; } // Flag that tell whether this license allow usage after expiration (expiration mode)
        }

        [Flags]
        public enum Feature : ushort
        {
            None = 0,
            Management = 1 << 0,
            Integrated = 1 << 1,
            Full = Management | Integrated,
        }
    }
}
