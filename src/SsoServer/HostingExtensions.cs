using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SsoServer.Data;
using SsoServer.Data.Seeding;
using SsoServer.Models;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using SsoServer.Infrastructures;
using SsoServer.Models.Users;
using SsoServer.Services;
using System.Globalization;

namespace SsoServer
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            // The UI pages are in the Pages folder
            builder.Services.AddRazorPages();

            // Setup Controllers support for API endpoints
            builder.Services.AddControllers();

            // The call to MigrationsAssembly(�) later tells Entity Framework that the host project will contain the migrations. This is necessary since the host project is in a different assembly than the one that contains the DbContext classes.
            var migrationsAssembly = typeof(Program).Assembly.GetName().Name;
            string dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(connectionString, npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(Math.Pow(2, 3)),
                            errorCodesToAdd: null);
                });

            }, ServiceLifetime.Transient);

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
            {

                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 3;
                opt.Password.RequiredUniqueChars = 1;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Identity Server
            builder.Services
                .AddIdentityServer(options =>
                {
                    options.IssuerUri = "https://auth.westley.it";
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                    options.EmitStaticAudienceClaim = true;
                })
                // Store configuration data: resources and clients, etc.
                .AddConfigurationStore(options => 
                {
                    options.ConfigureDbContext = b => b.UseNpgsql(dbConnectionString, npgsqlOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(Math.Pow(2, 3)),
                                errorCodesToAdd: null);
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                    });
                })
                // Store operational data that IdentityServer produces: tokens, codes, and consents, etc.
                .AddOperationalStore(options => 
                {
                    options.ConfigureDbContext = b => b.UseNpgsql(dbConnectionString, npgsqlOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(Math.Pow(2, 3)),
                                errorCodesToAdd: null);
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                    });
                })
                // Adds the integration layer to allow IdentityServer to access the user data for the ASP.NET Core Identity user database (configured above). This is needed when IdentityServer must add claims for the users into tokens.
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<IdentityClaimsProfileService>();
            
            
            builder.Services
                .AddAuthentication(option =>    
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(option =>
                {
                    option.Authority = "https://auth.westley.it";
                    option.Audience = "api1";
                    option.TokenValidationParameters.ValidateAudience = false;
                    option.TokenValidationParameters.ValidateIssuer = false;
                    option.TokenValidationParameters.ValidateIssuerSigningKey = false;
                });

            // Enable token validation for local APIs
            builder.Services.AddLocalApiAuthentication();

            // Use mediator design pattern to reduce coupling between classes while allowing communication between them.
            builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

            // Swagger for User API
            builder.Services.AddSwaggerGen(setup =>
                {
                    // Include 'SecurityScheme' to use JWT Authentication
                    var jwtSecurityScheme = new OpenApiSecurityScheme
                    {
                        BearerFormat = "JWT",
                        Name = "JWT Authentication",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                        Reference = new OpenApiReference
                        {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme
                        }
                    };

                    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        { jwtSecurityScheme, Array.Empty<string>() }
                    });

                });
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IClientService, ClientService>();
            builder.Services.AddAutoMapper(typeof(HostingExtensions));



            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.Use((context, next) =>
            {
                context.Request.Scheme = "https";
                return next();
            });
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Automatically apply db migrations and seed development databases for development purpose.
                // In production, this should either be done manually, or done through a UI application.
                app.EnsureAspNetCoreIdentityDatabaseIsSeeded(true);
                app.EnsureIdentityServerConfigurationDbIsSeeded(true);
                app.EnsureIdentityServerPersistedGrantDbIsSeeded(true);

                // Swagger UI for development
                app.UseSwagger();
                

                app.UseSwaggerUI();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages()
                .RequireAuthorization();

            // Map API endpoints following MVC Controller convention
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

            return app;
        }

    }
}