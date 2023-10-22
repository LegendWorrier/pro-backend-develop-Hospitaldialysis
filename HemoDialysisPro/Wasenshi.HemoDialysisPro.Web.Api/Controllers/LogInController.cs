using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Wasenshi.HemoDialysisPro.Web.Api.Models;
using Microsoft.AspNetCore.Identity;
using Wasenshi.HemoDialysisPro.Models;
using System.Linq;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [AllowAnonymous]
    public class LogInController : Controller
    {
        private readonly UserManager<User> userManager;

        public LogInController(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        public IActionResult Index(string ReturnUrl = "/")
        {
            LoginModel objLoginModel = new()
            {
                ReturnUrl = ReturnUrl
            };
            return View("LogIn", objLoginModel);
        }

        [HttpGet]
        public IActionResult Denied()
        {
            ViewBag.Message = "Current Credential has no permission. Please use another account.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel objLoginModel)
        {
            if (ModelState.IsValid)
            {
                var user = await this.userManager.FindByNameAsync(objLoginModel.UserName);
                if (user == null)
                {
                    ViewBag.Message = "Invalid Credential";
                    return View(objLoginModel);
                }
                else
                {
                    if (!await userManager.CheckPasswordAsync(user, objLoginModel.Password))
                    {
                        ViewBag.Message = "Invalid Credential";
                        return View(objLoginModel);
                    }

                    var roles = await userManager.GetRolesAsync(user);
                    var claims = new List<Claim>() {
                        new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.Id)),
                        new Claim(ClaimTypes.Name, user.UserName)
                    };
                    var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));
                    claims.AddRange(roleClaims);
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    //SignInAsync is a Extension method for Sign in a principal for the specified scheme.    
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
                    {
                        IsPersistent = true
                    });
                    return LocalRedirect(objLoginModel.ReturnUrl);
                }
            }
            return View(objLoginModel);
        }
    }
}
