using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using WebAPI.Migrations;
using WebAPI.Models;
using WebAPI.Setup;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
            _proxied = _configuration["PROXIED"];
        }

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly string _proxied;

        private void ConfigureMySQL(DbContextOptionsBuilder options, string connectionName)
        {
            string connectionString = _configuration.GetConnectionString(connectionName);
            ServerVersion serverVersion = new MySqlServerVersion("5.7.33");
            options.UseMySql(connectionString, serverVersion);
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            DataType.SetDBMS(_configuration["DBMS"]);

            if (DataType.UseMySQL)
            {
                services.AddDbContext<ApplicationDbContext>(options => ConfigureMySQL(options, "MySQL_Connection"));
            }
            else if (DataType.UseSQLite)
            {
                services.AddDbContext<ApplicationDbContext>(options => options
                    .UseSqlite(_configuration.GetConnectionString("SQLite_Connection")));
            }
            else if (DataType.UseSQLServer)
            {
                services.AddDbContext<ApplicationDbContext>(options => options
                    .UseSqlServer(_configuration.GetConnectionString("SQLServer_Connection")));
            }
            else throw new SystemException("No DBMS specified!");
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            SecurityKey securityKey;
            string secretKey = _configuration["JwtSecurity:SecretKey"];
            string publicKey = _configuration["JwtSecurity:PublicKey"];

            if (!string.IsNullOrEmpty(secretKey))
            {
                securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            }
            else if (!string.IsNullOrEmpty(publicKey))
            {
                RSA rsa = RSA.Create(); // note: must not use 'using', must not dispose.
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
                securityKey = new RsaSecurityKey(rsa);
            }
            else throw new SystemException("No security key specified!");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                bool validate = !_configuration.GetValue<bool>("JwtSecurity:Shorten");
                string[] audiences = _configuration["JwtSecurity:Audiences"].Split(';');
                string issuer = _configuration["JwtSecurity:Issuer"];

                var tokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = validate,
                    ValidateAudience = validate,
                    ValidAudiences = audiences,
                    ValidIssuer = issuer,

                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    //ClockSkew = TimeSpan.FromMinutes(1),

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey
                };

                if (string.IsNullOrEmpty(secretKey)) // if using RSA
                    tokenValidationParameters.CryptoProviderFactory =
                        new CryptoProviderFactory() { CacheSignatureProviders = false };

                options.SaveToken = true;
                options.RequireHttpsMetadata = !_env.IsDevelopment();
                options.TokenValidationParameters = tokenValidationParameters;
            });
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    Name = "Authorization",

                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Enter your bearer token below, which you obtain from signing-in",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureDatabase(services);

            ConfigureAuthentication(services);

            services.AddControllers();

            ConfigureSwagger(services);

            services.AddAutoMapper(c => c.AddProfile<MappingProfile>(), typeof(Startup));

            services.AddSingleton<Services.EmailService>();
            services.AddSingleton<Services.PushNotificationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (_proxied != null)
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            app.UseMiddleware<OptionsMiddleware>();

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1"));
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
