using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Wasenshi.AuthPolicy.Filter
{
    internal class AuthorizationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();
            if (resultContext.Exception != null && resultContext.Exception is UnauthorizedException)
            {
                resultContext.ExceptionHandled = true;
                resultContext.Result = new ForbidResult();
            }
        }
    }
}
