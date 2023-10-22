using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.PluginBase
{
    /// <summary>
    /// This plugin module allow you to intercept the auth process.
    /// You can use <see cref="AuthHandlerBase"/> as the base class.
    /// </summary>
    public interface IAuthHandler
    {
        delegate Task<bool> OnAuthenticateHandler(string username, string password);
        delegate Task<UserResult> OnLogInHandler(string username, string password);

        /// <summary>
        /// You can handle the login/authentication process and return the user data from existing system. This will auto-register the user to hemopro system also.
        /// <br/>
        /// <br/>
        /// Note: If the login/authentication failed, hemopro will fallback to original process and check for it's own user repository instead.
        /// </summary>
        OnLogInHandler OnLogIn { get; }
        
        /// <summary>
        /// You can handle the authentication check on your system side instead. Or else hemopro will use it's own user repository checking.
        /// </summary>
        OnAuthenticateHandler OnAuthen { get; }
        /// <summary>
        /// You can intercept the register process, and block it if some condition is not met.
        /// Or perhaps use hemopro as a proxy and allow it to pass registration to your existing system.
        /// </summary>
        Task<AuthResult> OnRegister(IUser user, string password);
        /// <summary>
        /// Intercept edit user process.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="user"></param>
        Task<AuthResult> OnEdit(string username, IUser user);
        /// <summary>
        /// Intercept delete user process.
        /// </summary>
        /// <param name="username"></param>
        Task<AuthResult> OnDelete(string username);
        /// <summary>
        /// Intercept user change password process.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="oldPassword"></param>
        /// <param name="password"></param>
        Task<AuthResult> OnChangePassword(string username, string oldPassword, string password);
    }

    public class AuthResult
    {
        public virtual bool Success => Error == null && ErrorDetail == null;
        public Exception? Error { get; set; }
        public string? ErrorDetail { get; set; }

        public static readonly AuthResult SuccessResult = new();
    }

    /// <summary>
    /// Base class for intercepting the auth process.
    /// </summary>
    public abstract class AuthHandlerBase : IAuthHandler
    {
        protected readonly ILogger<AuthHandlerBase> logger;

        protected AuthHandlerBase(ILogger<AuthHandlerBase> logger)
        {
            this.logger = logger;
        }

        public virtual IAuthHandler.OnLogInHandler OnLogIn => null;
        public virtual IAuthHandler.OnAuthenticateHandler OnAuthen => null;

        public virtual Task<AuthResult> OnRegister(IUser user, string password)
        {
            logger.LogInformation("[PLUGIN] no handler override for on register.");
            return Task.FromResult(AuthResult.SuccessResult);
        }

        public virtual Task<AuthResult> OnChangePassword(string username, string oldPassword, string password)
        {
            logger.LogInformation("[PLUGIN] no handler override for on change password.");
            return Task.FromResult(AuthResult.SuccessResult);
        }

        public virtual Task<AuthResult> OnDelete(string username)
        {
            logger.LogInformation("[PLUGIN] no handler override for on delete user.");
            return Task.FromResult(AuthResult.SuccessResult);
        }

        public virtual Task<AuthResult> OnEdit(string username, IUser user)
        {
            logger.LogInformation("[PLUGIN] no handler override for on edit user.");
            return Task.FromResult(AuthResult.SuccessResult);
        }
    }
}
