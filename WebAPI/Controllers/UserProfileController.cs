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
        public async Task<ActionResult<UserProfileDTO>> GetUserProfile()
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

                var userProfileDto = _mapper.Map<UserProfileDTO>(userProfile);
                return userProfileDto;
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpDelete]
        public async Task<ActionResult> DeleteUserProfile(long id)
        {
            try
            {
                UserProfile userProfile = dbc.UserProfiles.Find(id);

                if (userProfile == null)
                    return NotFound("User profile not found.");

                if (userProfile.Identity != UserIdentity)
                    return Forbid("User profile does not match!");

                userProfile.DateDeleted = DateTimeOffset.UtcNow;
                await dbc.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [HttpPost]
        public Task<ActionResult<UserProfileDTO>> PostUserProfile(UserProfileDTO userProfileDto)
        {
            return SetUserProfile(userProfileDto, false);
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPut]
        public Task<ActionResult<UserProfileDTO>> PutUserProfile(UserProfileDTO userProfileDto)
        {
            return SetUserProfile(userProfileDto, true);
        }

        private async Task<ActionResult<UserProfileDTO>> SetUserProfile(UserProfileDTO userProfileDto, bool onPut)
        {
            try
            {
                var validationResults = ValidateObject(userProfileDto);
                if (validationResults != null)
                    return BadRequest(validationResults);

                if (userProfileDto.DateDeleted != null)
                    return Forbid("Cannot delete user profile!");

                if (userProfileDto.Identity != UserIdentity)
                    return Forbid("Identity does not match!");

                var userProfile = await dbc.UserProfiles.FirstOrDefaultAsync(u => u.Identity == UserIdentity);
                if (userProfile == null || userProfile.DateDeleted != null)
                {
                    if (onPut) // else onPost
                        return NotFound("User profile not found.");

                    if (userProfileDto.Id != 0)
                        return BadRequest("Id must be 0!");

                    if (userProfileDto.DateDeleted != null)
                        return Forbid("Cannot set DateDeleted!");

                    userProfileDto.DateCreated = DateTimeOffset.UtcNow;

                    if (userProfile == null)
                    {
                        userProfile = _mapper.Map<UserProfile>(userProfileDto);
                        dbc.UserProfiles.Add(userProfile);
                        await dbc.SaveChangesAsync(); // get Id
                        userProfileDto.Id = userProfile.Id; // set Id
                    }
                    else
                    {
                        userProfileDto.Id = userProfile.Id; // set Id
                        _mapper.Map(userProfileDto, userProfile);
                        await dbc.SaveChangesAsync();
                    }

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
