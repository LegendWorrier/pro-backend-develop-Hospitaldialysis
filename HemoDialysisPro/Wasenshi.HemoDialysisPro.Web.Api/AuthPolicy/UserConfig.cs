using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using Wasenshi.AuthPolicy;
using Wasenshi.AuthPolicy.Options;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Web.Api.AuthPolicy
{
    public class UserConfig : DefaultUserConfigBase<User, Guid>
    {
        public UserConfig(UserManager<User> userManager, IOptionsMonitor<AuthPolicyOptions> option) : base(userManager, option)
        {
        }
        protected override IConverter Converter => _converter;

        private MyConverter _converter = new MyConverter();
        private class MyConverter : IConverter
        {
            public Converter<string, Guid> ConvertToId => (s) => Guid.Parse(s);

            public Converter<Guid, string> ConvertToString => (s) => s.ToString();
        }
    }
}
