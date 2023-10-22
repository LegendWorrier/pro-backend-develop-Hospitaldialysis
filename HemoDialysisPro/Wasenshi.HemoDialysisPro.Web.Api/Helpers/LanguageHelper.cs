using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Wasenshi.HemoDialysisPro.Web.Api.Helpers
{
    public static class LanguageHelper
    {
        public static void SetLangCookie(this HttpResponse response)
        {
            const string cookieName = "Lang";
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(CultureInfo.CurrentUICulture));
            response.Cookies.Append(cookieName, cookieValue);
        }

    }
}
