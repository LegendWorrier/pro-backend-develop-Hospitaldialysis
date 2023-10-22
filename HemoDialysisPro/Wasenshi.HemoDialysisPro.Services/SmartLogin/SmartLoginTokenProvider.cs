using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services.SmartLogin
{
    public class SmartLoginTokenProvider : DataProtectorTokenProvider<User>
    {
        public SmartLoginTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<DataProtectionTokenProviderOptions> options,
            ILogger<SmartLoginTokenProvider> logger) : base(dataProtectionProvider, options, logger)
        {
        }
    }

    public static class SmartLogin
    {
        public static readonly string ID = "SmartLogin";
    }
}
