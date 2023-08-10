    using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SsoServer.Constants;
using SsoServer.Models;
using System.Security.Claims;
    using SsoServer.Models.Users;

    namespace SsoServer.Data.Seeding
{
    public static class AspNetCoreIdentitySeeder
    {
        /// <summary>
        ///     Ensure the Identity database is created and seeded.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="autoMigrateDatabase"></param>
        public static void EnsureAspNetCoreIdentityDatabaseIsSeeded(this IApplicationBuilder builder, bool autoMigrateDatabase)
        {
            using var serviceScope = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

            if (autoMigrateDatabase)
            {
                dbContext.Database.Migrate();
            }

            var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
            SeedRoles(roleManager);

            var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
            SeedUsers(userManager);
        }

        /// <summary>
        ///     Seed the user roles
        /// </summary>
        /// <param name="roleManager"></param>
        private static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            TryAddRole(roleManager, UserRoles.Administrator);
            TryAddRole(roleManager, UserRoles.SuperAdministrator);
            TryAddRole(roleManager, UserRoles.User);

        }

        /// <summary>
        ///     Try to add a role
        /// </summary>
        /// <param name="roleManager"></param>
        /// <param name="roleName"></param>
        private static void TryAddRole(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!roleManager.RoleExistsAsync(roleName).Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = roleName;

                IdentityResult roleResult = roleManager.CreateAsync(role).Result;

                Log.Information($"Successfully seeded role: {roleName}");
            }
        }

        /// <summary>
        ///     Seed the initial users
        /// </summary>
        /// <param name="userManager"></param>
        private static void SeedUsers(UserManager<ApplicationUser> userManager)
        {

            TryAddUser(userManager, "admin@rubrik.it", ".admin.", "admin@rubrik.it", "Admin", "Test", new string[] { Constants.UserRoles.SuperAdministrator});
            
            // Seed more users here.
        }

        /// <summary>
        ///     Try to add a user
        /// </summary>
        private static void TryAddUser(UserManager<ApplicationUser> userManager, string username, string password, string email, string firstname, string lastname, string[] roles)
        {
            // Only seed if user does not exist yet
            if (userManager.FindByNameAsync(username).Result == null)
            {
                // Build the user (ASP.NET Core Identity User)
                ApplicationUser user = new()
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true
                };
                // Create user
                IdentityResult result = userManager.CreateAsync(user, password).Result;

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                if (result.Succeeded)
                {
                    // Add user roles
                    foreach (var role in roles)
                    {
                        userManager.AddToRoleAsync(user, role).Wait();
                    }

                    // Add user claims
                    result = userManager.AddClaimsAsync(user, new Claim[]
                        {
                            new(JwtClaimTypes.GivenName, firstname),
                            new(JwtClaimTypes.FamilyName, lastname),
                            new(JwtClaimTypes.ClientId, "rutino"),
                            new(JwtClaimTypes.ClientId, "torchiara"),
                        }).Result;

                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                }

                Log.Information($"Successfully seeded user: {username}");
            }
            else
            {
                Log.Debug($"{username} already exists");
            }
        }
    }
}
