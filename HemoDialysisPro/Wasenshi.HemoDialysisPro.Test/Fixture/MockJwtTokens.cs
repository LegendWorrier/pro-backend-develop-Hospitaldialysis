using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Test.Fixture
{
    public static class MockJwtTokens
    {
        public static string Issuer { get; } = Guid.NewGuid().ToString();
        public static SecurityKey SecurityKey { get; }
        public static SigningCredentials SigningCredentials { get; }

        private static readonly JwtSecurityTokenHandler s_tokenHandler = new JwtSecurityTokenHandler();
        private static readonly RandomNumberGenerator s_rng = RandomNumberGenerator.Create();
        private static readonly byte[] s_key = new byte[32];

        static MockJwtTokens()
        {
            s_rng.GetBytes(s_key);
            var key = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            SecurityKey = new SymmetricSecurityKey(key);
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
        }

        public static string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Issuer,
                claims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: SigningCredentials);
            return s_tokenHandler.WriteToken(token);
        }
    }

    public static class MockJwtTokensExtension
    {
        public static IServiceCollection ConfigureMockJwt(this IServiceCollection services)
        {
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, x =>
            {
                var config = new OpenIdConnectConfiguration()
                {
                    Issuer = MockJwtTokens.Issuer
                };

                config.SigningKeys.Add(MockJwtTokens.SecurityKey);
                x.Configuration = config;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false
                };
            });
            return services;
        }

        public static void SetMockBearerToken(this HttpClient client, string username)
        {
            SetMockBearerToken(client, username, new string[0], "");
        }

        public static void SetMockBearerToken(this HttpClient client, string username, string[] roles, dynamic claim = null)
        {
            SetMockBearerToken(client, username, roles, null, claim);
        }

        public static void SetMockBearerToken(this HttpClient client, string username, string[] roles, object id, dynamic claim = null)
        {
            if (id == null)
            {
                id = Guid.NewGuid();
            }
            // Set token claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            };
            var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));
            claims.AddRange(roleClaims);
            if (claim == null)
            {
                claim = new { };
            }
            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expando;
            if (claim is IDictionary<string, object> source)
            {
                foreach (var item in source)
                    dictionary.Add(item.Key, item.Value);
            }
            else
            {
                foreach (var property in claim.GetType().GetProperties())
                    dictionary.Add(property.Name, property.GetValue(claim));
            }
            if (!dictionary.ContainsKey("unit") && !roles.Contains(Roles.PowerAdmin))
            {
                dictionary.Add("unit", new object[] { -1 });
            }
            foreach (var item in dictionary)
            {
                if (item.Value is IList)
                {
                    foreach (var value in item.Value as IList)
                    {
                        claims.Add(new Claim(item.Key, value.ToString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(item.Key, item.Value.ToString()));
                }
            }
            // Generate
            var token = MockJwtTokens.GenerateJwtToken(claims);
            // Add token to client
            AddTokenToClient(client, token);
        }

        private static void AddTokenToClient(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        public static void WithNoToken(this HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = null;
        }
    }
}
