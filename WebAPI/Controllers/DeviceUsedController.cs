using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebAPI.Services;
using WebAPI.Models;
using Global.Models;
using Global.Enums;
using AutoMapper;

namespace WebAPI.Controllers
{
    public class DeviceUsedController : BaseController<DeviceUsedController>
    {
        private readonly PushNotificationService _pushNotificationService;

        public DeviceUsedController(
            PushNotificationService pushNotificationService,
            ApplicationDbContext context,
            ILogger<DeviceUsedController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _pushNotificationService = pushNotificationService;
        }

        [ProducesResponseType(typeof(DeviceUsedDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpPut]
        public async Task<ActionResult<DeviceUsedDTO>> PutDeviceUsed(DevicePlatformEnum platform, string language, string timezone)
        {
            try
            {
                var userProfile = await dbc.Users
                                        .Include(x => x.PhotoFile)
                                        .Include(x => x.WallpaperFile)
                                        .FirstOrDefaultAsync(x => x.Id == UserId);

                if (userProfile == null)
                    return NotFound("User profile not found.");

                var utcNow = DateTimeOffset.UtcNow;

                var deviceUsed = await dbc.UserDevices
                                        .Where(x =>
                                            x.UserId == userProfile.Id &&
                                            x.Platform == platform)
                                        .FirstOrDefaultAsync();
                if (deviceUsed == null)
                {
                    deviceUsed = new UserDevice
                    {
                        UserId = userProfile.Id,
                        Platform = platform,
                        DateCreated = utcNow
                    };
                    dbc.UserDevices.Add(deviceUsed);
                }
                else if (deviceUsed.DateDeleted != null)
                {
                    deviceUsed.DateDeleted = null;
                }
                deviceUsed.Language = language;
                deviceUsed.Timezone = timezone;
                deviceUsed.DateUpdated = utcNow;
                dbc.SaveChanges();

                deviceUsed.User = userProfile;
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

                UserDevice deviceUsed = dbc.UserDevices.Find(id);

                if (deviceUsed?.UserId == UserId)
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

                var deviceUsed = dbc.UserDevices.Find(deviceUsedId);

                if (deviceUsed == null)
                    return NotFound($"DeviceUsed {deviceUsedId} not found.");

                if (deviceUsed.UserId != UserId)
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
