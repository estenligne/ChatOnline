﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using WebAPI.Models;
using WebAPI.Setup;
using System;
using System.Security.Cryptography;

namespace WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        private void ConfigureAuthentication(IServiceCollection services)
        {
            string publicKey = _configuration["JwtSecurity:PublicKey"];
            RSA rsa = RSA.Create(); // note: must not use 'using' here.
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = !_env.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = _configuration["URLS"],
                    ValidIssuer = _configuration["JwtSecurity:Issuer"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    CryptoProviderFactory = new CryptoProviderFactory() { CacheSignatureProviders = false }
                };
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (Migrations.DataType.UseMySQL)
            {
                services.AddDbContext<AccountDbContext>(options => options
                    .UseMySQL(_configuration.GetConnectionString("Account_MySQL_Connection")));

                services.AddDbContext<ApplicationDbContext>(options => options
                    .UseMySQL(_configuration.GetConnectionString("ChatOnline_MySQL_Connection")));
            }
            else
            {
                services.AddDbContext<AccountDbContext>(options => options
                    .UseSqlServer(_configuration.GetConnectionString("Account_SQLServer_Connection")));

                services.AddDbContext<ApplicationDbContext>(options => options
                    .UseSqlServer(_configuration.GetConnectionString("ChatOnline_SQLServer_Connection")));
            }

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<AccountDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // SignIn settings.
                options.SignIn.RequireConfirmedAccount = true;
            });

            ConfigureAuthentication(services);

            services.AddControllers();

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" });
            });

            services.AddAutoMapper(c => c.AddProfile<MappingProfile>(), typeof(Startup));

            services.AddSingleton<Services.EmailService>();
            services.AddSingleton<Services.PushNotificationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<OptionsMiddleware>();

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1"));
            }
            else app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
