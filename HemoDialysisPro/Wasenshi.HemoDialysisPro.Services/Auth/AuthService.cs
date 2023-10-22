using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Implementation;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserUnitOfWork _userUnit;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger logger;
        private readonly AuthConfig _config;

        private readonly bool isLocalServer;

        public AuthService(IUserUnitOfWork userUnit, UserManager<User> userManager, RoleManager<Role> roleManager, IOptionsMonitor<AuthConfig> config, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userUnit = userUnit;
            _userManager = userManager;
            _roleManager = roleManager;
            this.logger = logger;
            _config = config.CurrentValue;

            isLocalServer = configuration.GetValue<bool>("Authentication:Local");
        }

        public async Task<UserResult> AuthenticateAsync(string username, string password)
        {
            User user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return null;
            }

            bool result = await _userManager.CheckPasswordAsync(user, password);
            if (result)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var units = _userUnit.User.GetUserUnitMap().Where(x => x.UserId == user.Id).ToList();
                user.Units = units;
                return new UserResult
                {
                    User = user,
                    Roles = roles
                };
            }

            return new UserResult();
        }

        public async Task<AuthResponse> GenerateToken(UserResult userResult, string ipAddress)
        {
            string token = await GenerateAccessToken(userResult);
            RefreshToken refreshToken = GenerateRefreshToken(ipAddress);
            // save refresh token
            userResult.User.Units = null;
            userResult.User.RefreshTokens.Add(refreshToken);
            _userUnit.User.Update(userResult.User as User);
            _userUnit.Complete();

            return new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken.Token,
                Expire = _config.Expire
            };
        }

        public async Task<AuthResponse> RefreshToken(string refreshToken, string ipAddress)
        {
            var user = _userUnit.User.GetAll().SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == refreshToken)) as User;
            // return null if no user found with token
            if (user == null) { logger.LogInformation("refresh token not found."); return null; }

            var token = user.RefreshTokens.Single(x => x.Token == refreshToken);
            // deny potential hacker attempt
            if (!isLocalServer && ipAddress != token.CreatedByIp) { logger.LogWarning($"Potential hacking attempt from ip: {ipAddress} user: {user.UserName}[{user.Id}] token: {refreshToken}"); return null; }

            // return null if token is no longer active
            if (!token.IsActive)
            {
                // Detect replay attack
                if (!token.IsExpired || !string.IsNullOrWhiteSpace(token.ReplacedByToken)) // this means the token is reused / replayed
                {
                    if (token.RevokedByIp == ipAddress) // log out case
                    {
                        return null;
                    }

                    logger.LogWarning($"token reuse detected. user: {user.UserName}[{user.Id}] token: {refreshToken}");
                    // deny all the subsequence tokens in family, forcing re-authen.
                    token.Revoked = DateTime.UtcNow;
                    RefreshToken subToken = user.RefreshTokens.SingleOrDefault(x => x.Token == token.ReplacedByToken);
                    if (subToken != null) subToken.Revoked = DateTime.UtcNow;
                    while (subToken != null && !string.IsNullOrWhiteSpace(subToken.ReplacedByToken))
                    {
                        subToken = user.RefreshTokens.SingleOrDefault(x => x.Token == subToken.ReplacedByToken);
                        if (subToken != null) subToken.Revoked = DateTime.UtcNow;
                    }
                    _userUnit.User.Update(user);
                    _userUnit.Complete();
                }
                else { logger.LogInformation($"token expired."); }

                return null;
            }

            // replace old refresh token with a new one and save
            RefreshToken newRefreshToken = GenerateRefreshToken(ipAddress);
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);

            _userUnit.User.Update(user);
            _userUnit.Complete();

            // generate new access token
            var roles = await _userManager.GetRolesAsync(user);
            var userResult = new UserResult
            {
                User = user,
                Roles = roles
            };
            string accessToken = await GenerateAccessToken(userResult);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                Expire = _config.Expire
            };
        }

        public async Task<IdentityResult> RegisterUserAsync(User user, string password, IList<string> roles)
        {
            var checkResult = await VerifyRoles(roles);
            if (!checkResult.Succeeded)
            {
                return checkResult;
            }

            var userCreateResult = await _userManager.CreateAsync(user, password);
            if (!userCreateResult.Succeeded)
            {
                return userCreateResult;
            }

            var result = await _userManager.AddToRolesAsync(user, roles);
            return result;
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var user = _userUnit.User.GetAll().SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (user == null) return false;

            RefreshToken refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            logger.LogInformation("User logged out/revoked the token.");

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _userUnit.User.Update(user);
            _userUnit.Complete();

            return true;
        }

        public async Task<bool> RevokeUserAsync(User user)
        {
            var token = user.RefreshTokens.FirstOrDefault(x => DateTime.UtcNow < x.Expires && x.Revoked == null);
            if (token == null) return false;

            token.Revoked = DateTime.UtcNow;

            _userUnit.User.Update(user);
            _userUnit.Complete();

            return true;
        }

        public bool VerifyUnit(ClaimsPrincipal user, IEnumerable<int> unitList, bool allowEmptyList = true)
        {
            if (user.IsInRole(Roles.PowerAdmin)) // Bypass unit check for poweradmin
            {
                return true;
            }

            return !unitList?.Except(user.GetUnitList()).Any() ?? allowEmptyList;
        }

        public async Task<IdentityResult> VerifyRoles(IList<string> roles)
        {
            //check and reject invalid role first
            foreach (var role in roles)
            {
                var checkPass = await _roleManager.RoleExistsAsync(role);
                if (!checkPass)
                {
                    var error = _roleManager.ErrorDescriber.InvalidRoleName(role);
                    return IdentityResult.Failed(error);
                }
            }

            return IdentityResult.Success;
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddSeconds(_config.LifeTime),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        private async Task<string> GenerateAccessToken(UserResult userResult)
        {
            User user = userResult.User as User;
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? ""),
                new Claim(ClaimTypes.Surname, user.LastName ?? ""),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? "")
            };

            var roleClaims = userResult.Roles.Select(r => new Claim(ClaimTypes.Role, r));
            claims.AddRange(roleClaims);
            var unitClaims = userResult.User.Units?.Select(u => new Claim("unit", u.UnitId.ToString())) ?? Enumerable.Empty<Claim>();
            claims.AddRange(unitClaims);

            List<Claim> permissionList = new();
            foreach (var item in userResult.Roles)
            {
                var role = await _roleManager.FindByNameAsync(item);
                var permissions = await _roleManager.GetClaimsAsync(role);
                permissionList.AddRange(permissions);
            }
            permissionList.AddRange(await _userManager.GetClaimsAsync(user));
            claims.AddRange(permissionList.Distinct(new ClaimsComparer()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expire_in = _config.Expire;
            var expires = DateTime.UtcNow.AddSeconds(expire_in);

            var token = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Issuer,
                claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private sealed class ClaimsComparer : IEqualityComparer<Claim>
        {
            public bool Equals(Claim x, Claim y)
            {
                return x.Type == y.Type && x.Value == y.Value;
            }

            public int GetHashCode([DisallowNull] Claim obj)
            {
                return obj.Type.GetHashCode() ^ obj.Value.GetHashCode();
            }
        }
    }
}
