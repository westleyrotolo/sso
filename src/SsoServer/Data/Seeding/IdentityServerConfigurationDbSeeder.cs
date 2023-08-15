﻿using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace SsoServer.Data.Seeding
{
    public static class IdentityServerConfigurationDbSeeder
    {
        private static readonly string QEApi = nameof(QEApi);

        public static IEnumerable<IdentityResource> IdentityResourcesToSeed =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopesToSeed =>
            new ApiScope[]
            {
                new ApiScope("api1", displayName: "API 1"),
                new ApiScope("api2", displayName: "API 2"),
                // API endpoints in the same application hosting IdentityServer
                new ApiScope(IdentityServerConstants.LocalApi.ScopeName, displayName: IdentityServerConstants.LocalApi.ScopeName),
                new ApiScope(QEApi, displayName: "Farmland Web API"),
            };

        private static IEnumerable<Client> ClientsToSeed =>
            new Client[]
            {
                new()
                {
                    ClientId = "rutino",
                    ClientName = "Comune Di Rutino",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    // Secret code
                    AllowedScopes =
                    {
                        "api1",
                        "api2",
                        QEApi
                    },
                    
                    RequireClientSecret = true,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                },
                new()
                {
                    ClientId = "torchiara",
                    ClientName = "Comune Di Torchiara",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    // Secret code
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                    AllowedScopes = {
                        "api1",
                        "api2",
                        QEApi
                    },
                    RequireClientSecret = true
                },
                new()
                {
                    ClientId = "lustra",
                    ClientName = "Comune Di Lustra",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    // Secret code
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                    AllowedScopes = {
                        "api1",
                        "api2",
                        QEApi,
                    },
                    
                    RequireClientSecret = true
                },
                // interactive client using code flow + pkce
                new()
                {
                    ClientId = "interactive.client",
                    ClientName = "Interactive Client",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    // where to redirect to after login
                    RedirectUris = { "https://auth.westley.it/signin-oidc" },
                    FrontChannelLogoutUri = "https://auth.westley.it/signout-oidc",
                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://auth.westley.it/signout-callback-oidc" },

                    // In order to support refresh token, offline access is required.
                    AllowOfflineAccess = true,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api2",
                        // Access to local API in the same application hosting SSO server
                        IdentityServerConstants.LocalApi.ScopeName,
                        QEApi
                    },
                }
            };

        /// <summary>
        /// Seed the Identity Server configuration db (identity resources, clients, etc)
        /// </summary>
        public static void EnsureIdentityServerConfigurationDbIsSeeded(this IApplicationBuilder builder, bool autoMigrateDatabase)
        {
            using (var serviceScope = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<ConfigurationDbContext>();

                if (autoMigrateDatabase)
                {
                   // dbContext.Database.Migrate();
                }

                // An identity resource is a named group of claims about a user that can be requested using the scope parameter.
                SeedIdentityResources(dbContext);
                // The scope of api access that a client requests.
                SeedApiScopes(dbContext);
                // Clients represent applications that can request tokens from your SSO server.
                SeedClients(dbContext);
            }
        }

        private static void SeedIdentityResources(ConfigurationDbContext dbContext)
        {
            foreach (var resource in IdentityResourcesToSeed)
            {
                TryAddIdentityResource(dbContext, resource);
            }
        }

        private static void TryAddIdentityResource(ConfigurationDbContext dbContext, IdentityResource resource)
        {
            if (!dbContext.IdentityResources.Any(x => x.Name == resource.Name))
            {
                dbContext.IdentityResources.Add(resource.ToEntity());
            }

            dbContext.SaveChanges();

            Log.Information($"Successfully seeded identity resource: {resource.Name}");

        }

        private static void SeedApiScopes(ConfigurationDbContext dbContext)
        {
            foreach (var resource in ApiScopesToSeed)
            {
                TryAddApiScope(dbContext, resource);
            }
        }

        private static void TryAddApiScope(ConfigurationDbContext dbContext, ApiScope scope)
        {
            if (!dbContext.ApiScopes.Any(x => x.Name == scope.Name))
            {
                dbContext.ApiScopes.Add(scope.ToEntity());
            }

            dbContext.SaveChanges();

            Log.Information($"Successfully seeded api scope: {scope.Name}");

        }

        private static void SeedClients(ConfigurationDbContext dbContext)
        {
            foreach (var client in ClientsToSeed)
            {
                TryAddClient(dbContext, client);
            }
        }

        private static void TryAddClient(ConfigurationDbContext dbContext, Client client)
        {
            if (!dbContext.Clients.Any(x => x.ClientId == client.ClientId))
            {
                dbContext.Clients.Add(client.ToEntity());
            }

            dbContext.SaveChanges();

            Log.Information($"Successfully seeded client: {client.ClientName}");

        }
    }
}
