using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Utils
{
    public class HttpHelper : IHttpHelper
    {
        private readonly IHttpContextAccessor contextAccessor;

        public HttpHelper(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public ClaimsPrincipal GetClaimsPrincipal()
        {
            return contextAccessor.HttpContext?.User;
        }

        async Task<string> IHttpHelper.GetTokenAsync()
        {
            return await contextAccessor.HttpContext?.GetTokenAsync("access_token");
        }
    }

    public interface IHttpHelper
    {
        Task<string> GetTokenAsync();
        ClaimsPrincipal GetClaimsPrincipal();
    }

    public static class HttpHelperExtension
    {
        public static IServiceCollection AddHttpHelper(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<IHttpHelper, HttpHelper>();
            return services;
        }
    }
}
