using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Global.Enums;
using Global.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    public class DeviceUsedController : BaseController<DeviceUsedController>
    {
        private readonly IConfiguration _configuration;
        private readonly PushNotificationService _pushNotificationService;

        public DeviceUsedController(
            IConfiguration configuration,
            PushNotificationService pushNotificationService,
            ApplicationDbContext context,
            ILogger<DeviceUsedController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _configuration = configuration;
            _pushNotificationService = pushNotificationService;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(typeof(DeviceUsedDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpPut]
        public async Task<ActionResult<DeviceUsedDTO>> PutDeviceUsed(DevicePlatformEnum platform, string language, string timezone)
        {
            try
            {
                long accountId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var userProfile = await dbc.UserProfiles
                                        .Include(x => x.PhotoFile)
                                        .Include(x => x.WallpaperFile)
                                        .FirstOrDefaultAsync(x => x.Id == accountId);

                if (userProfile == null)
                    return NotFound("User profile not found.");

                var utcNow = DateTimeOffset.UtcNow;

                var deviceUsed = await dbc.DevicesUsed
                                        .Where(x =>
                                            x.UserProfileId == userProfile.Id &&
                                            x.Platform == platform)
                                        .FirstOrDefaultAsync();
                if (deviceUsed == null)
                {
                    deviceUsed = new DeviceUsed
                    {
                        UserProfileId = userProfile.Id,
                        Platform = platform,
                        DateCreated = utcNow
                    };
                    dbc.DevicesUsed.Add(deviceUsed);
                }
                else if (deviceUsed.DateDeleted != null)
                {
                    deviceUsed.DateDeleted = null;
                }

                deviceUsed.Language = language;
                deviceUsed.Timezone = timezone;
                deviceUsed.DateUpdated = utcNow;
                dbc.SaveChanges();

                byte[] secretKey = Encoding.UTF8.GetBytes(_configuration["JwtSecurity:SecretKey"]);
                string token = CustomAuthenticationHandler.BuildJWT(deviceUsed, secretKey, Response);

                deviceUsed.UserProfile = userProfile;
                var deviceUsedDto = _mapper.Map<DeviceUsedDTO>(deviceUsed);
                return Ok(deviceUsedDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpDelete]
        public ActionResult DeleteDeviceUsed(long id)
        {
            try
            {
                string tag = $"User {UserId} {User.Identity.Name} on device {id}";

                DeviceUsed deviceUsed = dbc.DevicesUsed.Find(id);

                if (deviceUsed?.UserProfileId == UserId)
                {
                    if (deviceUsed.DateDeleted == null)
                    {
                        deviceUsed.DateDeleted = DateTimeOffset.UtcNow;
                        dbc.SaveChanges();

                        _logger.LogInformation($"{tag} has signed out.");
                    }
                    else _logger.LogWarning($"{tag} already signed out.");
                }
                else _logger.LogError($"{tag} not found or not valid.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPatch]
        [Route(nameof(RegisterFcmToken))]
        public ActionResult RegisterFcmToken(long deviceUsedId, string fcmToken)
        {
            try
            {
                if (string.IsNullOrEmpty(fcmToken))
                    return BadRequest("FcmToken not provided.");

                if (fcmToken.Length > 1023)
                    return BadRequest("FcmToken length must be < 1024.");

                var deviceUsed = dbc.DevicesUsed.Find(deviceUsedId);

                if (deviceUsed == null)
                    return NotFound($"DeviceUsed {deviceUsedId} not found.");

                if (deviceUsed.UserProfileId != UserId)
                    return Forbid("User profile does not match!");

                if (deviceUsed.DateDeleted != null)
                    return Conflict($"User already signed out of this device!");

                deviceUsed.PushNotificationToken = fcmToken;
                deviceUsed.DateTokenProvided = DateTimeOffset.UtcNow;

                dbc.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
