global using Wasenshi.HemoDialysisPro.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Redis;
using System;
using System.Linq;
using System.Text.Json;
using Wasenshi.AuthPolicy.Utillities;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Services.Initialization
{
    public static class Initialization
    {
        public static void InitializeData(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            bool firstInit = !context.Database.CanConnect();
            Console.WriteLine("Start EF Migrating..");
            context.Database.Migrate();
            Console.WriteLine("EF Migration completed.");
#if !TEST
            InitPermission(scope.ServiceProvider, firstInit);
            InitRedisConfig(app);
#endif

            //AddRoles(context);
            //AddBuitInAdminUser(app, context);
            //Console.WriteLine("Finish adding built-in admin");
        }

        private static void InitRedisConfig(IApplicationBuilder app)
        {
            using var redis = app.ApplicationServices.CreateScope().ServiceProvider.GetService<IRedisClient>();
            var setting = app.ApplicationServices.GetRequiredService<IWritableOptions<GlobalSetting>>();
            redis.Set(Common.SEE_OWN_PATIENT_ONLY, setting.Value.Patient?.DoctorCanSeeOwnPatientOnly);
        }

        private static void InitPermission(IServiceProvider services, bool firstInit)
        {
            var rolemanager = services.GetRequiredService<RoleManager<Role>>();
            rolemanager.AddPermissionToRole(Roles.PowerAdmin, Permissions.GLOBAL).ConfigureAwait(true).GetAwaiter().GetResult();
            rolemanager.AddPermissionToRole(Roles.Admin, Permissions.USER).ConfigureAwait(true).GetAwaiter().GetResult();
            if (firstInit)
            {
                rolemanager.AddPermissionToRole(Roles.Admin,
                    Permissions.BASIC,
                    Permissions.ASSESSMENT,
                    Permissions.DIALYSIS,
                    Permissions.Hemosheet.SETTING,
                    Permissions.LABEXAM,
                    Permissions.SCHEDULE).ConfigureAwait(true).GetAwaiter().GetResult();
            }
        }

        private static void AddRoles(ApplicationDbContext context)
        {
            var roleStore = new RoleStore<Role, ApplicationDbContext, Guid>(context);
            foreach (string role in Roles.AllRoles)
            {
                if (!roleStore.Context.Roles.Any(x => x.Name == role))
                {
                    Console.WriteLine("Adding role: " + role);
                    var result = roleStore.CreateAsync(new Role(role)).ConfigureAwait(false).GetAwaiter().GetResult();
                    Console.WriteLine(JsonSerializer.Serialize(result));
                    Console.WriteLine("Adding role: " + role + " -> Completed.");
                }
            }
        }

        private static void AddBuitInAdminUser(IApplicationBuilder app, ApplicationDbContext context)
        {
            // Add root admin
            var userStore = new UserStore<User, Role, ApplicationDbContext, Guid>(context);
            var hasRootAdmin = userStore.Users.Any(x => x.UserName == DataSeed.AdminUsername);
            if (!hasRootAdmin)
            {
                Console.WriteLine("Adding root admin...");
                var user = new User
                {
                    FirstName = "root",
                    LastName = "admin",
                    UserName = DataSeed.AdminUsername,
                    NormalizedUserName = DataSeed.AdminUsername.ToUpper(),
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D")
                };

                var password = new PasswordHasher<User>();
                var hashed = password.HashPassword(user, DataSeed.AdminPassword);
                user.PasswordHash = hashed;

                var result = userStore.CreateAsync(user).ConfigureAwait(false).GetAwaiter().GetResult();
                Console.WriteLine("Root admin created.");
                AssignRoles(app.ApplicationServices, DataSeed.AdminUsername, Roles.AllRoles);
                Console.WriteLine("Assign roles to root admin -> complete.");
            }
        }

        private static void AssignRoles(IServiceProvider services, string username, string[] roles)
        {
            using var scope = services.CreateScope();
            UserManager<User> _userManager = scope.ServiceProvider.GetService<UserManager<User>>();
            User user = _userManager.FindByNameAsync(username).ConfigureAwait(false).GetAwaiter().GetResult();
            var result = _userManager.AddToRolesAsync(user, roles).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
