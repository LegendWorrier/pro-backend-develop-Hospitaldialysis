using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Wasenshi.AuthPolicy.Test
{
    public class UtilTest
    {
        ClaimsPrincipal claimsPrincipal;
        public UtilTest()
        {
            claimsPrincipal = new ClaimsPrincipal(new[]
            {
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "Test"),
                    new Claim(ClaimTypes.NameIdentifier, "Test"),
                    new Claim(ClaimTypes.Role, "admin"),
                    new Claim(ClaimTypes.Role, "doctor"),
                })
            });
        }

        [Fact]
        public void IsInRole_single_value_should_success()
        {
            string roleList = "doctor, user";

            claimsPrincipal.IsInAnyRole(new[] { roleList }).Should().BeTrue();

            roleList = "man, girl";

            claimsPrincipal.IsInAnyRole(new[] { roleList }).Should().BeFalse();
        }

        [Fact]
        public void IsInRole_multi_values_should_success()
        {
            string[] roleList = new[] { "admin", "user" };

            claimsPrincipal.IsInAnyRole(roleList).Should().BeTrue();

            roleList = new[] { "man, girl" };

            claimsPrincipal.IsInAnyRole(roleList).Should().BeFalse();
        }

        [Fact]
        public void IsInRole_Null_value_should_return_false_instead_of_error()
        {
            claimsPrincipal.IsInAnyRole(null).Should().BeFalse();
        }
    }
}
