using Auth.Service.API.Entities;
using Auth.Service.API.Entities.Context;
using Auth.Service.API.Interfaces;
using Auth.Service.API.Models;
using Auth.Service.API.Services;
using AutoMapper;
using CaseSolutionsTokenValidationParameters;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Auth.Service.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
          
        }

        public IConfiguration Configuration { get; }
        public ILogger<Startup> Logger { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string section = "AppSettings";
            string assemblyName = "Auth.Service.API";
            IConfigurationSection configuration = Configuration.GetSection(section);
            services.Configure<AppSettings>(configuration);
            AppSettings appsettings = configuration.Get<AppSettings>();

            services.AddDbContext<UserContext>(options =>
            {
                options.UseSqlServer(appsettings.AuthDbConnectionString,
                    migrationOptions =>
                    {
                        migrationOptions.MigrationsAssembly(assemblyName);
                    });

                Logger.LogInformation("Database connection established");

            });

            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appsettings.Secret));
            SigningCredentials signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = appsettings.Issuer;
                options.Audience = appsettings.Audience;
                options.SigningCredentials = signingCredentials;
            });

            IdentityBuilder builder = services.AddIdentityCore<User>(o =>
            {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            });
            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);
            builder
                .AddEntityFrameworkStores<UserContext>()
                .AddDefaultTokenProviders()
                .AddRoles<IdentityRole>();

            Logger.LogInformation("Authentication configuration has finsihed");

            services.AddValidationParameters(
              appsettings.Issuer,
              appsettings.Audience,
              _signingKey
              );

            Logger.LogInformation("Validation is setup.");

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IJwtService, JwtService>();

            Logger.LogInformation("Services has been added");

            services.AddAutoMapper();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                Logger.LogInformation("In Development environment");
            }
            else
            {
               
                app.UseHsts();
            }

            //Add logging:
            //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.2

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
