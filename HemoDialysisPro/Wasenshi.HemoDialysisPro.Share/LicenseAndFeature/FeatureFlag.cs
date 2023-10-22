using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using static Wasenshi.HemoDialysisPro.Share.LicenseManager;

namespace Wasenshi.HemoDialysisPro.Share
{
    /// <summary>
    /// This class is for protecting the app from hacker/pirate. (in exhange for a little lost of performance and efficiency)
    /// </summary>
    public static class FeatureFlag
    {
        // All the names are meaningless. But keep obfuscated and redundant to tolerate reverse-engineer and memory/cheat engine attack.

        private static bool PARAM55; // Integrated
        private static bool PARAM22; // Integrated
        private static bool PARAM33; // Integrated

        private static bool PARAM45; // Management
        private static bool PARAM24; // Management
        private static bool PARAM32; // Management

        // these are isExpire flag
        private static bool DISABLE1 = false;
        private static bool DISABLE2 = false;
        private static bool DISABLE3 = false;
        // allow expire mode
        private static bool DISABLE4 = false;
        private static bool DISABLE5 = false;
        private static bool DISABLE6 = false;


        private static ILogger logger;
        public static void SetLogger(ILogger logger)
        {
            FeatureFlag.logger = logger;
        }

        public static void SetupFeatureFlags(Feature features)
        {
            if (features == 0)
            {
                logger?.LogInformation("This is invalid license!");
                DISABLE1 = true;
                DISABLE2 = true;
                DISABLE3 = true;
                return;
            }

            if (features.HasFlag(Feature.Integrated))
            {
                PARAM22 = true;
                PARAM33 = true;
                PARAM55 = true;
            }
            if (features.HasFlag(Feature.Management))
            {
                PARAM45 = true;
                PARAM24 = true;
                PARAM32 = true;
            }

            // If we have more than 2 feature groups in the future, then refactor this.
            string licenseType;
            if (features.HasFlag(Feature.Full))
            {
                licenseType = "Full";
            }
            else if (features.HasFlag(Feature.Management))
            {
                licenseType = "Management (Basic)";
            }
            else
            {
                licenseType = "Integrated";
            }

            logger?.LogInformation("This is {LicenseType} license.", licenseType);

            if (ExpiredMode)
            {
                DISABLE1 = true;
                DISABLE2 = true;
                DISABLE3 = true;
            }

            if (AllowExpireMode)
            {
                DISABLE4 = true;
                DISABLE5 = true;
                DISABLE6 = true;
            }
        }

        public static bool HasIntegrated()
        {
            switch (RandomNumberGenerator.GetInt32(3))
            {
                case 0:
                    {
                        return Check1();
                    }
                case 1:
                    {
                        return Check2();
                    }
                case 2:
                    {
                        return Check3();
                    }
                default:
                    throw new InvalidProgramException("Hacker! propbably.");
            }
        }

        public static bool HasManagement()
        {
            switch (RandomNumberGenerator.GetInt32(3))
            {
                case 0:
                    {
                        return Check4();
                    }
                case 1:
                    {
                        return Check5();
                    }
                case 2:
                    {
                        return Check6();
                    }
                default:
                    throw new InvalidProgramException("Hacker! propbably.");
            }
        }

        public static bool IsDisabled()
        {
            switch (RandomNumberGenerator.GetInt32(3))
            {
                case 0:
                    {
                        return !Check96() && IsExpired();
                    }
                case 1:
                    {
                        return !Check95() && IsExpired();
                    }
                case 2:
                    {
                        return !Check94() && IsExpired();
                    }
                default:
                    throw new InvalidProgramException("Hacker! propbably.");
            }
        }

        public static bool IsExpired()
        {
            switch (RandomNumberGenerator.GetInt32(3))
            {
                case 0:
                    {
                        return Check99();
                    }
                case 1:
                    {
                        return Check98();
                    }
                case 2:
                    {
                        return Check97();
                    }
                default:
                    throw new InvalidProgramException("Hacker! propbably.");
            }
        }
        // ========================= Integrated ================================
        /// <summary>
        /// Integrated
        /// </summary>
        /// <returns></returns>
        private static bool Check1()
        {
            return PARAM55 && (RandomNumberGenerator.GetInt32(1) == 1 ? PARAM22 : PARAM33);
        }
        /// <summary>
        /// Integrated
        /// </summary>
        /// <returns></returns>
        private static bool Check2()
        {
            return PARAM22 && (RandomNumberGenerator.GetInt32(1) == 1 ? PARAM55 : PARAM33);
        }
        /// <summary>
        /// Integrated
        /// </summary>
        /// <returns></returns>
        private static bool Check3()
        {
            return PARAM33 && (RandomNumberGenerator.GetInt32(1) == 1 ? PARAM22 : PARAM55);
        }
        // ========================= Management ================================
        /// <summary>
        /// Management
        /// </summary>
        /// <returns></returns>
        private static bool Check4()
        {
            return PARAM45 && (RandomNumberGenerator.GetInt32(1) == 1 ? PARAM32 : PARAM24);
        }
        /// <summary>
        /// Management
        /// </summary>
        /// <returns></returns>
        private static bool Check5()
        {
            return PARAM24 && (RandomNumberGenerator.GetInt32(1) == 1 ? PARAM45 : PARAM32);
        }
        /// <summary>
        /// Management
        /// </summary>
        /// <returns></returns>
        private static bool Check6()
        {
            return PARAM32 && (RandomNumberGenerator.GetInt32(1) == 1 ? PARAM24 : PARAM45);
        }

        // ========================= Expired ================================
        private static bool Check99()
        {
            return DISABLE1 && (RandomNumberGenerator.GetInt32(1) == 1 ? DISABLE2 : DISABLE3);
        }

        private static bool Check98()
        {
            return DISABLE2 && (RandomNumberGenerator.GetInt32(1) == 1 ? DISABLE1 : DISABLE3);
        }

        private static bool Check97()
        {
            return DISABLE3 && (RandomNumberGenerator.GetInt32(1) == 1 ? DISABLE2 : DISABLE1);
        }
        // --------------- expire mode allow
        private static bool Check96()
        {
            return DISABLE4 && (RandomNumberGenerator.GetInt32(1) == 1 ? DISABLE5 : DISABLE6);
        }

        private static bool Check95()
        {
            return DISABLE5 && (RandomNumberGenerator.GetInt32(1) == 1 ? DISABLE4 : DISABLE6);
        }

        private static bool Check94()
        {
            return DISABLE6 && (RandomNumberGenerator.GetInt32(1) == 1 ? DISABLE5 : DISABLE4);
        }
    }
}
