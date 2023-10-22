using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services.SmartLogin
{
    public class OneTimeTokenProvider : DataProtectorTokenProvider<User>
    {
        public OneTimeTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<OneTimeTokenProviderOptions> options,
            ILogger<OneTimeTokenProvider> logger) : base(dataProtectionProvider, options, logger)
        {
        }
    }

    public class OneTimeTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public OneTimeTokenProviderOptions()
        {
            Name = nameof(OneTimeTokenProvider);
            TokenLifespan = TimeSpan.FromMinutes(2);
        }
    }

    public static class OneTimeToken
    {
        public static readonly string ID = "OneTimeToken";
    }
}
