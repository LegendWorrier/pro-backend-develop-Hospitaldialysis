using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.AuthPolicy.AuthHandlers;
using Xunit;

namespace Wasenshi.AuthPolicy.Test
{
    public class FieldEditPermissionHandlerTest
    {
        FieldEditPermissionHandler handler;
        public FieldEditPermissionHandlerTest()
        {
            var option = new AuthPolicyOptions
            {
                GlobalPermission = "global"
            };
            var optionMonitorMock = new Mock<IOptionsMonitor<AuthPolicyOptions>>();
            optionMonitorMock.Setup(x => x.CurrentValue).Returns(option);
            handler = new FieldEditPermissionHandler(optionMonitorMock.Object);
        }
        private ClaimsPrincipal GetUser(params string[] roles)
        {
            var claims = roles.Select(x => new Claim(ClaimTypes.Role, x));
            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        private class TestRoleModel
        {
            [RoleRestrict("poweruser")]
            public string prop1 { get; set; }
            [RoleForbid("user")]
            public string prop2 { get; set; }
            public string prop3 { get; set; }
            [RoleRestrict("user", "builder")]
            public int? integer { get; set; }
            [RoleForbid("user", "builder", "poweruser")]
            public DateTime? datetime { get; set; }

            public NestedModel Nested { get; set; } = new NestedModel();
            public class NestedModel
            {
                [RoleRestrict("user")]
                public string restrict { get; set; }
            }
            public List<NestedModel> List { get; set; } = new List<NestedModel>();
        }

        [Fact]
        public void When_NoEditData_Should_Pass()
        {
            var data = new TestRoleModel();
            //check with anonymous user
            var user = GetUser();
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeTrue();
        }

        [Fact]
        public void When_EditRestrictData_WithNoRole_Should_Fail()
        {
            var data = new TestRoleModel
            {
                prop1 = "something"
            };
            //check with anonymous user
            var user = GetUser();
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeFalse();
        }

        [Fact]
        public void When_EditForbidRoleData_WithNoRole_Should_Pass()
        {
            var data = new TestRoleModel
            {
                prop2 = "forbid"
            };
            //check with anonymous user
            var user = GetUser();
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeTrue();
        }

        [Fact]
        public void When_EditForbidRoleData_WithRole_Should_Fail()
        {
            var data = new TestRoleModel
            {
                prop2 = "forbid"
            };
            //forbid role
            var user = GetUser("user");
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeFalse();
            //anything but forbid role, should pass
            user = GetUser("poweruser", "builder", "admin");
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeTrue();
            //has forbid role and also other than forbid role, should pass
            user = GetUser("poweruser", "user");
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeTrue();
        }

        [Fact]
        public void When_EditOnlyNeutralData_Should_Pass()
        {
            var data = new TestRoleModel
            {
                prop3 = "something"
            };
            //check with anonymous user
            var user = GetUser();
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeTrue();
        }

        [Fact]
        public void When_EditRestrictValue_WithNoRole_Should_Fail()
        {
            var data = new TestRoleModel
            {
                integer = 555
            };
            //check with anonymous user
            var user = GetUser();
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeFalse();
        }

        [Fact]
        public void When_EditRestrictValue_WithRole_Should_Pass()
        {
            var data = new TestRoleModel
            {
                integer = 555
            };
            //check with anonymous user
            var user = GetUser("builder");
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeTrue();
        }

        [Fact]
        public void When_EditForbidValue_WithNoRole_Should_Pass()
        {
            var data = new TestRoleModel
            {
                datetime = DateTime.Now
            };
            //check with anonymous user
            var user = GetUser();
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeTrue();

            user = GetUser("otherrole");
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeTrue();
        }

        [Fact]
        public void When_EditRestrictNested_WithNoRole_Should_Fail()
        {
            var data = new TestRoleModel();
            data.Nested.restrict = "edited";
            //check with anonymous user
            var user = GetUser();
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeFalse();
        }

        [Fact]
        public void When_EditRestrictList_WithNoRole_Should_Fail()
        {
            var data = new TestRoleModel();
            data.List.Add(new TestRoleModel.NestedModel());
            data.List.Add(new TestRoleModel.NestedModel { restrict = "edit" });
            //check with anonymous user
            var user = GetUser();
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeFalse();
        }

        //Specail case : forbid anonymous
        class ForbidAnonymousModel
        {
            [RoleForbid]
            public float? prop1 { get; set; }
            public int? prop2 { get; set; }
        }

        [Fact]
        public void When_EditForbidAnonymous_Should_Fail()
        {
            var data = new ForbidAnonymousModel { prop2 = 123 };
            var user = GetUser();
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeTrue();

            data = new ForbidAnonymousModel { prop1 = 22.55f };
            handler.CheckRestrictOrForbid(user, data)
                .Should().BeFalse();
        }

    }
}
