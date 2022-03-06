using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using Global.Helpers;
using Global.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class AccountController : BaseController<AccountController>
    {
        private readonly IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.EmailService _emailService;

        public AccountController(
            IConfiguration configuration,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            Services.EmailService emailService,
            ApplicationDbContext context,
            ILogger<AccountController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
        }

        /// <summary>
        /// Endpoint to get details of the currently signed-in user
        /// </summary>
        [HttpGet]
        [Route(nameof(GetUser))]
        public async Task<ActionResult<ApplicationUserDTO>> GetUser()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var userDto = _mapper.Map<ApplicationUserDTO>(user);
            return userDto;
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(IEnumerable<IdentityError>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        [AllowAnonymous]
        [HttpPost]
        [Route(nameof(Register))]
        public async Task<ActionResult<ApplicationUserDTO>> Register(ApplicationUserDTO userDto)
        {
            try
            {
                string userName = GetUserName(userDto);

                if (!ModelState.IsValid)
                    return BadRequest(new ValidationProblemDetails(ModelState));

                var user = await _userManager.FindByNameAsync(userName);
                if (user != null)
                {
                    if (User.Identity.IsAuthenticated && User.IsInRole(Role.KnownServer))
                    {
                        return Ok(_mapper.Map<ApplicationUserDTO>(user));
                    }
                    return Conflict($"User account {userName} already exists.");
                }

                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = userDto.Email,
                    PhoneNumber = userDto.PhoneNumber,
                    DateCreated = DateTimeOffset.UtcNow,
                };

                var result = await _userManager.CreateAsync(user, userDto.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User account {userName} created successfully.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        if (userName == user.PhoneNumber)
                            await SendConfirmationOTP(user);
                        else await SendConfirmationEmail(user);

                        userDto = _mapper.Map<ApplicationUserDTO>(user);
                        return CreatedAtAction(nameof(SignIn), null, userDto);
                    }
                    else return NoContent();
                }
                else
                {
                    var errors = JsonConvert.SerializeObject(result.Errors);
                    var userDtoStr = JsonConvert.SerializeObject(userDto);
                    _logger.LogWarning($"Email registration failed. userDto: {userDtoStr} errors: {errors}");
                    return BadRequest(result.Errors);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private string GetUserName(ApplicationUserDTO userDto)
        {
            string userName = string.IsNullOrEmpty(userDto.PhoneNumber) ? userDto.Email : userDto.PhoneNumber;

            if (string.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError(nameof(userDto.Email), "Provide an email address or a phone number");
                ModelState.AddModelError(nameof(userDto.PhoneNumber), "Provide a phone number or an email address");
            }
            return userName;
        }

        private async Task SendConfirmationOTP(ApplicationUser user)
        {
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        private async Task SendConfirmationEmail(ApplicationUser user)
        {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string domainName = $"{this.Request.Scheme}://{this.Request.Host}";
            string controller = ControllerContext.ActionDescriptor.ControllerName;
            string path = $"{domainName}/api/{controller}/{nameof(ConfirmEmail)}";
            string email = System.Web.HttpUtility.UrlEncode(user.Email);
            token = System.Web.HttpUtility.UrlEncode(token);
            string href = $"{path}?email={email}&token={token}";
            string body = "<h2>Thank you for registering!</h2>\n";
            body += $"<p>Please confirm your account by <a href='{href}'>clicking here</a>.</p>\n";
            await _emailService.SendEmailAsync(user.Email, "Confirm Your Account", body);
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [AllowAnonymous]
        [HttpGet]
        [Route(nameof(ConfirmEmail))]
        public async Task<ActionResult> ConfirmEmail(string email, string token)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return NotFound($"User account {email} not found.");

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User account {email} confirmed.");
                    return Ok("Succeeded!");
                }
                else
                {
                    var errors = JsonConvert.SerializeObject(result.Errors);
                    _logger.LogWarning($"Email {email} confirmation failed. token: {token} errors: {errors}");
                    return BadRequest(result.Errors);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route(nameof(Authenticate))]
        public Task<ActionResult> Authenticate(ApplicationUserDTO userDto)
        {
            return SignIn(userDto);
        }

        [ProducesResponseType(typeof(ApplicationUserDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        [HttpPost]
        [Route(nameof(SignIn))]
        public async Task<ActionResult> SignIn(ApplicationUserDTO userDto)
        {
            try
            {
                string userName = GetUserName(userDto);

                if (!ModelState.IsValid)
                    return BadRequest(new ValidationProblemDetails(ModelState));

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return NotFound($"User account {userName} not found.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    if (userName == user.PhoneNumber)
                    {
                        if (!user.PhoneNumberConfirmed)
                        {
                            await SendConfirmationOTP(user);
                            return Unauthorized("Please first confirm your account using the OTP sent to your phone number.");
                        }
                    }
                    else
                    {
                        if (!user.EmailConfirmed)
                        {
                            await SendConfirmationEmail(user);
                            return Unauthorized("Please first confirm your account using the email sent. Check your Spam/Junk folder.");
                        }
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(userName, userDto.Password, userDto.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.Id} {user.UserName} has authenticated.");

                    var roles = await _userManager.GetRolesAsync(user);
                    string jwt = BuildJWT(user.Id, user.UserName, roles, userDto.Authorization);

                    userDto = _mapper.Map<ApplicationUserDTO>(user);
                    userDto.Authorization = "Bearer " + jwt;

                    return Ok(userDto);
                }
                else if (result.RequiresTwoFactor)
                {
                    return Accepted(result);
                }
                else if (result.IsLockedOut)
                {
                    string message = $"User account {userName} is locked out.";
                    _logger.LogWarning(message);
                    return Forbid(message);
                }
                else if (result.IsNotAllowed)
                {
                    string message = $"User account {userName} is not allowed.";
                    _logger.LogWarning(message);
                    return Forbid(message);
                }
                else
                {
                    return BadRequest("Password not correct!");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private string BuildJWT(long userId, string userName, IList<string> roles, string duration)
        {
            SigningCredentials signingCredentials;
            using RSA rsa = RSA.Create();

            string key = _configuration["JwtSecurity:SecretKey"];
            if (!string.IsNullOrEmpty(key))
            {
                var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
                signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            }
            else
            {
                key = _configuration["JwtSecurity:PrivateKey"];
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(key), out _);
                var securityKey = new RsaSecurityKey(rsa);
                signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
                signingCredentials.CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false };
            }

            var claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));

            if (!_configuration.GetValue<bool>("JwtSecurity:Shorten"))
            {
                string issuer = _configuration["JwtSecurity:Issuer"];

                string audience = Request.Headers["Origin"];
                if (string.IsNullOrEmpty(audience))
                    audience = issuer;

                claims.Add(new Claim(JwtRegisteredClaimNames.Iss, issuer));
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));

                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, userName));

                foreach (string role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            int minutes = string.IsNullOrEmpty(duration) ? 24 * 60 : int.Parse(duration);

            var token = new JwtSecurityToken(claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: signingCredentials);

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpDelete]
        [Route(nameof(SignOut))]
        public new async Task<ActionResult> SignOut()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation($"User {UserId} {User.Identity.Name} has signed out.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotImplemented)]
        [AllowAnonymous]
        [HttpPatch]
        [Route(nameof(ForgotPassword))]
        public ActionResult ForgotPassword(string emailAddress, string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
            }
            else if (!string.IsNullOrEmpty(emailAddress))
            {
                if (emailAddress == "email@address.com")
                {
                    string target = string.IsNullOrEmpty(phoneNumber) ? "email address" : "phone number";
                    return Ok($"A confirmation message to reset your password has been sent to your {target}!");
                }
            }
            else return BadRequest("No email address nor phone number provided.");

            return StatusCode((int)HttpStatusCode.NotImplemented, "The feature to reset password is not yet available!");
        }
    }
}
