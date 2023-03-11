using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Models;

namespace WebAPI.Services
{
    public class CustomAuthenticationHandler : AuthenticationHandler<CustomAuthenticationSchemeOptions>
    {
        private readonly ApplicationDbContext dbc;

        public CustomAuthenticationHandler(
            IOptionsMonitor<CustomAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ApplicationDbContext dbc)
            : base(options, logger, encoder, clock)
        {
            this.dbc = dbc;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                string authorization = Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorization))
                    return AuthenticateResult.Fail("No Authorization header");

                if (!authorization.StartsWith(Scheme.Name + " "))
                    return AuthenticateResult.Fail($"Expected '{Scheme.Name}' scheme");

                string token = authorization.Substring(Scheme.Name.Length + 1);
                var jwt = new JwtSecurityToken(token);

                byte[] bytesToSign = Encoding.ASCII.GetBytes(jwt.RawHeader + "." + jwt.RawPayload);
                byte[] hash = new HMACSHA256(Options.SecretKey).ComputeHash(bytesToSign);
                string computedSignature = Base64UrlEncoder.Encode(hash);

                if (computedSignature != jwt.RawSignature)
                    return AuthenticateResult.Fail($"Invalid '{Scheme.Name}' token");

                DateTime utcNow = DateTime.UtcNow;
                DateTime issuedAt = jwt.IssuedAt;

                if (utcNow > issuedAt + Options.TimeToRefresh)
                    return AuthenticateResult.Fail("Refresh capability has expired");

                if (utcNow > issuedAt + Options.TimeToAccess)
                {
                    object value = null;
                    jwt.Payload.TryGetValue(JwtRegisteredClaimNames.Sid, out value);
                    long userDeviceId = (long)value;

                    var userDevice = await dbc.DevicesUsed.FindAsync(userDeviceId);
                    token = BuildJWT(userDevice, Options.SecretKey, Response);

                    if (token == null)
                        return AuthenticateResult.Fail("Device has been signed out");

                    jwt = new JwtSecurityToken(token);
                }

                var identity = new ClaimsIdentity(jwt.Claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to authenticate");
                return AuthenticateResult.Fail(ex.Message);
            }
        }

        public const string SchemeName = "Custom";
        private static readonly Random random = new Random();

        public static string BuildJWT(DeviceUsed userDevice, byte[] secretKey, HttpResponse response)
        {
            if (userDevice == null || userDevice.DateDeleted != null)
                return null;

            var securityKey = new SymmetricSecurityKey(secretKey);
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userDevice.UserProfileId.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Sid, userDevice.Id.ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Jti, random.NextInt64().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };
            var jwt = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials);
            string token = new JwtSecurityTokenHandler().WriteToken(jwt);

            response.Headers["Set-Authorization"] = SchemeName + " " + token;
            return token;
        }

        public static long GetUserId(ClaimsPrincipal principal)
        {
            return long.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        }
    }

    public class CustomAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public byte[] SecretKey;
        public TimeSpan TimeToAccess;
        public TimeSpan TimeToRefresh;
    }
}
