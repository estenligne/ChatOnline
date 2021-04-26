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
                                            .Where(x => x.User.UserName == User.Identity.Name)
                                            .FirstOrDefaultAsync();

                if (userProfile == null)
                    return NotFound("User profile not found.");

                var deviceUsed = await dbc.DevicesUsed
                                        .Where(x =>
                                            x.DateDeleted == null &&
                                            x.UserProfileId == userProfile.Id &&
                                            x.DevicePlatform == devicePlatform)
                                        .FirstOrDefaultAsync();

                if (deviceUsed == null)
                {
                    deviceUsed = new DeviceUsed
                    {
                        UserProfileId = userProfile.Id,
                        DevicePlatform = devicePlatform,
                        DateCreated = DateTime.UtcNow,
                    };
                    dbc.DevicesUsed.Add(deviceUsed);
                    dbc.SaveChanges();
                }

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
                                    .Include(d => d.UserProfile.User)
                                    .FirstOrDefault(d => d.Id == deviceUsedId);

                if (deviceUsed == null)
                    return NotFound($"DeviceUsed {deviceUsedId} not found.");

                if (deviceUsed.DateDeleted != null)
                    return Forbid($"User already logged out of this device!");

                if (deviceUsed.UserProfile.User.UserName != User.Identity.Name)
                    return Forbid("User profile does not match!");

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
        [Route(nameof(Send))]
        public async Task<ActionResult<List<PushNotificationOutcome>>> Send(
            [FromQuery] List<long> userProfileIds,
            [FromBody] PushNotificationDTO pushNotificationDto)
        {
            var outcomes = await _pushNotificationService.SendAsync(dbc, userProfileIds, pushNotificationDto);
            return Ok(outcomes);
        }
        #endif
    }
}
