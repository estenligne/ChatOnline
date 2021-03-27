using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebAPI.Models;
using Global.Models;
using AutoMapper;
using Newtonsoft.Json;

namespace WebAPI.Controllers
{
    public class UserController : BaseController<UserController>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.EmailService _emailService;

        public UserController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            Services.EmailService emailService,
            ApplicationDbContext context,
            ILogger<UserController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
        }

        /// <summary>
        /// Endpoint to get details of the currently logged in user
        /// </summary>
        [HttpGet]
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
                var email = userDto.Email;

                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                    return Conflict($"User account {email} already exists.");

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = userDto.PhoneNumber,
                };

                var result = await _userManager.CreateAsync(user, userDto.Password);
                if (result.Succeeded)
                {
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        await SendConfirmationEmail(user);
                    }

                    if (!string.IsNullOrWhiteSpace(userDto.ProfileName))
                    {
                        var userProfile = new UserProfile();
                        userProfile.UserId = user.Id;
                        userProfile.Username = userDto.ProfileName;
                        userProfile.DateCreated = DateTime.UtcNow;
                        dbc.UserProfiles.Add(userProfile);
                        dbc.SaveChanges();
                    }

                    _logger.LogInformation($"User account {email} created successfully.");
                    userDto = _mapper.Map<ApplicationUserDTO>(user);

                    return CreatedAtAction(nameof(Login), null, userDto);
                }
                else
                {
                    var errors = JsonConvert.SerializeObject(result.Errors);
                    var userDtoStr = JsonConvert.SerializeObject(userDto);
                    _logger.LogWarning($"Email registration failed. userDto: {userDtoStr} errors: {errors}");
                    return BadRequest(result.Errors);
                }
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private async Task SendConfirmationEmail(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var domainName = $"{this.Request.Scheme}://{this.Request.Host}";
            var path = $"{domainName}/api/User/{nameof(ConfirmEmail)}";
            var email = System.Web.HttpUtility.UrlEncode(user.Email);
            token = System.Web.HttpUtility.UrlEncode(token);
            var href = $"{path}?email={email}&token={token}";
            var body = $"<p>Please confirm your account by <a href='{href}'>clicking here</a>.</p>";
            _logger.LogDebug(body);
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

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        [HttpPost]
        [Route(nameof(Login))]
        public async Task<ActionResult> Login(ApplicationUserDTO userDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userDto.Email);
                if (user == null)
                    return NotFound($"User account {userDto.Email} not found.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount && !user.EmailConfirmed)
                {
                    await SendConfirmationEmail(user);
                    return Unauthorized("Please first confirm your account using the email sent. Check your Spam/Junk folder.");
                }

                var result = await _signInManager.PasswordSignInAsync(userDto.Email,
                                   userDto.Password, userDto.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User account {userDto.Email} has logged in.");
                    return NoContent();
                }
                if (result.RequiresTwoFactor)
                {
                    return Ok(result);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"User account {userDto.Email} is locked out.");
                    return Unauthorized($"User account {userDto.Email} is locked out.");
                }
                else
                {
                    return BadRequest("Invalid login attempt.");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPost]
        [Route(nameof(Logout))]
        public async Task<ActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation($"User {User.Identity.Name} logged out.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
