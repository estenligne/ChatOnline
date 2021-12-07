using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;
using Global.Models;
using AutoMapper;

namespace WebAPI.Controllers
{
    public class UserProfileController : BaseController<UserProfileController>
    {
        public UserProfileController(
            ApplicationDbContext context,
            ILogger<UserProfileController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        [HttpGet]
        public async Task<ActionResult<UserProfileDTO>> GetUserProfile(long id)
        {
            try
            {
                var userProfile = await dbc.UserProfiles
                                            .Include(u => u.PhotoFile)
                                            .FirstOrDefaultAsync(u => u.Id == id);

                if (userProfile == null)
                    return NotFound($"User profile {id} not found.");

                if (userProfile.PhotoFile?.DateDeleted != null)
                {
                    userProfile.PhotoFileId = null;
                    userProfile.PhotoFile = null;
                }

                var userProfileDto = _mapper.Map<UserProfileDTO>(userProfile);
                return Ok(userProfileDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpDelete]
        public async Task<ActionResult> DeleteUserProfile()
        {
            try
            {
                UserProfile userProfile = dbc.UserProfiles.Find(UserId);

                if (userProfile == null)
                    return NotFound("User profile not found.");

                userProfile.DateDeleted = DateTimeOffset.UtcNow;
                await dbc.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType(typeof(UserProfileDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [HttpPost]
        public Task<ActionResult> PostUserProfile(UserProfileDTO userProfileDto)
        {
            return SetUserProfile(userProfileDto, false);
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [HttpPut]
        public Task<ActionResult> PutUserProfile(UserProfileDTO userProfileDto)
        {
            return SetUserProfile(userProfileDto, true);
        }

        private async Task<ActionResult> SetUserProfile(UserProfileDTO userProfileDto, bool onPut)
        {
            try
            {
                var validationResults = ValidateObject(userProfileDto);
                if (validationResults != null)
                    return BadRequest(validationResults);

                if (userProfileDto.DateDeleted != null)
                    return Forbid("Cannot delete user profile!");

                var userProfile = dbc.UserProfiles.Find(UserId);
                if (userProfile == null || userProfile.DateDeleted != null)
                {
                    if (onPut) // else onPost
                        return NotFound("User profile not found.");

                    if (userProfileDto.Id != 0)
                        return BadRequest("Id must be 0!");

                    userProfileDto.Id = UserId;
                    userProfileDto.DateCreated = DateTimeOffset.UtcNow;

                    if (userProfile == null)
                    {
                        userProfile = _mapper.Map<UserProfile>(userProfileDto);
                        dbc.UserProfiles.Add(userProfile);
                    }
                    else _mapper.Map(userProfileDto, userProfile);

                    await dbc.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetUserProfile), null, userProfileDto);
                }
                else
                {
                    if (!onPut) // if onPost
                        return Conflict("User profile already exists.");

                    if (userProfileDto.Id != userProfile.Id)
                        return Forbid("Id does not match!");

                    if (userProfileDto.DateCreated != userProfile.DateCreated)
                        return Forbid("Cannot update DateCreated!");

                    userProfile = _mapper.Map(userProfileDto, userProfile);
                    await dbc.SaveChangesAsync();

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
