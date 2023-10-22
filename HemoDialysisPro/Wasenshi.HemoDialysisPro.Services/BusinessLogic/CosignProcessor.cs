using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.PluginBase;
using Wasenshi.HemoDialysisPro.PluginIntegrate;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public class CosignProcessor : ICosignProcessor
    {
        private readonly UserManager<User> userManager;
        private readonly IEnumerable<IAuthHandler> authPlugins;
        private readonly ILogger<CosignProcessor> logger;

        public CosignProcessor(UserManager<User> userManager, IEnumerable<IAuthHandler> authPlugins, ILogger<CosignProcessor> logger)
        {
            this.userManager = userManager;
            this.authPlugins = authPlugins;
            this.logger = logger;
        }

        public async Task<bool> ValidateCosignAsync<T>(Guid cosignUserId, string cosignPassword, T resource) where T : EntityBase
        {
            var user = await userManager.FindByIdAsync(cosignUserId.ToString());

            if (user == null || resource == null)
            {
                return false;
            }
            // Safe guard: cannot proof yourself
            if (user.Id == resource.CreatedBy)
            {
                throw new InvalidOperationException("Cannot assign the owner as a proof reader");
            }

            var pluginResult = await authPlugins.ExecutePlugins(async handler =>
            {
                if (handler.OnAuthen != null)
                {
                    var result = await handler.OnAuthen(user.UserName, cosignPassword);
                    if (result)
                    {
                        return result;
                    }
                }
                return (bool?)null;
            }, e => logger.LogError(e, "Plugin error at cosign request."));
            if (pluginResult == true)
            {
                return true;
            }

            // validate cosign password
            bool validate = await userManager.CheckPasswordAsync(user, cosignPassword);
            if (!validate)
            {
                return false;
            }

            return true;
        }
    }
}
