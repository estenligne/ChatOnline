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

        [HttpPost]
        [Route(nameof(GetOrCreate))]
        public async Task<ActionResult<DeviceUsedDTO>> GetOrCreate([FromQuery] DevicePlatformEnum devicePlatform)
        {
            try
            {
                var userProfile = await dbc.UserProfiles
                                            .Include(x => x.PhotoFile)
                                            .Include(x => x.WallpaperFile)
                                            .Where(x => x.Identity == UserIdentity)
                                            .FirstOrDefaultAsync();

                if (userProfile == null)
                    return NotFound("User profile not found.");

                var utcNow = DateTime.UtcNow;

                var deviceUsed = await dbc.DevicesUsed
                                        .Where(x =>
                                            x.UserProfileId == userProfile.Id &&
                                            x.DevicePlatform == devicePlatform)
                                        .FirstOrDefaultAsync();
                if (deviceUsed == null)
                {
                    deviceUsed = new DeviceUsed
                    {
                        UserProfileId = userProfile.Id,
                        DevicePlatform = devicePlatform,
                        DateCreated = utcNow
                    };
                    dbc.DevicesUsed.Add(deviceUsed);
                }
                else if (deviceUsed.DateDeleted != null)
                {
                    deviceUsed.DateDeleted = null;
                }
                deviceUsed.DateUpdated = utcNow;
                dbc.SaveChanges();

                deviceUsed.UserProfile = userProfile;
                var deviceUsedDto = _mapper.Map<DeviceUsedDTO>(deviceUsed);
                return deviceUsedDto;
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPost]
        [Route(nameof(RegisterFcmToken))]
        public ActionResult RegisterFcmToken(long deviceUsedId, string fcmToken)
        {
            try
            {
                if (string.IsNullOrEmpty(fcmToken))
                    return BadRequest("FcmToken not provided.");

                if (fcmToken.Length > 1023)
                    return BadRequest("FcmToken length must be < 1024.");

                var exists = dbc.DevicesUsed
                                .Where(d => d.DateDeleted == null && d.PushNotificationToken == fcmToken)
                                .FirstOrDefault();

                if (exists != null)
                {
                    if (exists.Id != deviceUsedId)
                    {
                        return Conflict("The push notification token is already registered.");
                    }
                    else return NoContent();
                }

                var deviceUsed = dbc.DevicesUsed
                                    .Include(d => d.UserProfile)
                                    .FirstOrDefault(d => d.Id == deviceUsedId);

                if (deviceUsed == null)
                    return NotFound($"DeviceUsed {deviceUsedId} not found.");

                if (deviceUsed.UserProfile.Identity != UserIdentity)
                    return Forbid("User profile does not match!");

                if (deviceUsed.DateDeleted != null)
                    return Forbid($"User already logged out of this device!");

                deviceUsed.PushNotificationToken = fcmToken;
                deviceUsed.DateTokenProvided = DateTime.UtcNow;

                dbc.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #if DEBUG
        [HttpPost]
        [Route(nameof(SendPushNotification))]
        public async Task<ActionResult<List<PushNotificationOutcome>>> SendPushNotification(
            [FromQuery] List<long> userProfileIds,
            [FromBody] PushNotificationDTO pushNotificationDto)
        {
            var outcomes = await _pushNotificationService.SendAsync(dbc, userProfileIds, pushNotificationDto);
            return Ok(outcomes);
        }
        #endif
    }
}
