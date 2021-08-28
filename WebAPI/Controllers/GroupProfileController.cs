using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;
using Global.Models;
using Global.Enums;
using AutoMapper;

namespace WebAPI.Controllers
{
    public class GroupProfileController : BaseController<GroupProfileController>
    {
        public GroupProfileController(
            ApplicationDbContext context,
            ILogger<GroupProfileController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        private Task<UserChatRoom> GetUserChatRoom(long groupProfileId)
        {
            return dbc.UserChatRooms
                    .Include(x => x.UserProfile)
                    .Where(x =>
                        x.UserProfile.Identity == UserIdentity &&
                        x.ChatRoom.GroupProfileId == groupProfileId)
                    .FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<ActionResult<GroupProfileDTO>> GetGroupProfile(long id)
        {
            try
            {
                var userChatRoom = await GetUserChatRoom(id);
                if (userChatRoom == null)
                    return NotFound("Group profile not found.");

                var groupProfile = await dbc.GroupProfiles
                                            .Include(x => x.PhotoFile)
                                            .Include(x => x.WallpaperFile)
                                            .FirstOrDefaultAsync(x => x.Id == id);

                var groupProfileDto = _mapper.Map<GroupProfileDTO>(groupProfile);

                if ((userChatRoom.UserRole & UserRoleEnum.GroupAdmin) == 0)
                    groupProfileDto.JoinToken = null; // only expose JoinToken to admins

                return groupProfileDto;
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpDelete]
        public async Task<ActionResult> DeleteGroupProfile(long id)
        {
            try
            {
                var userChatRoom = await GetUserChatRoom(id);
                if (userChatRoom == null)
                    return NotFound("Group profile not found.");

                var groupProfile = dbc.GroupProfiles.Find(id);
                if (groupProfile.CreatorId != userChatRoom.UserProfileId)
                    return Forbid("Cannot delete group profile!");

                groupProfile.DateDeleted = DateTime.UtcNow;
                await dbc.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPut]
        public async Task<ActionResult> PutGroupProfile(GroupProfileDTO groupProfileDto)
        {
            try
            {
                var validationResults = ValidateObject(groupProfileDto);
                if (validationResults != null)
                    return BadRequest(validationResults);

                var userChatRoom = await GetUserChatRoom(groupProfileDto.Id);
                if (userChatRoom == null)
                    return NotFound("Group profile not found.");

                if ((userChatRoom.UserRole & UserRoleEnum.GroupAdmin) == 0)
                    return Forbid("Cannot update group profile!");

                if (groupProfileDto.DateDeleted != null)
                    return Forbid("Cannot delete group profile!");

                var groupProfile = dbc.GroupProfiles.Find(groupProfileDto.Id);

                if (groupProfileDto.CreatorId != groupProfile.CreatorId)
                    return Forbid("Cannot update group creator!");

                if (groupProfileDto.JoinToken != groupProfile.JoinToken &&
                    groupProfileDto.CreatorId != userChatRoom.UserProfileId)
                    return Forbid("Cannot update group JoinToken!");

                if (groupProfileDto.DateCreated != groupProfile.DateCreated)
                    return Forbid("Cannot update group DateCreated!");

                groupProfile = _mapper.Map(groupProfileDto, groupProfile);
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
        public async Task<ActionResult<UserChatRoomDTO>> PostGroupProfile(GroupProfileDTO groupProfileDto)
        {
            try
            {
                var validationResults = ValidateObject(groupProfileDto);
                if (validationResults != null)
                    return BadRequest(validationResults);

                if (groupProfileDto.Id != 0)
                    return BadRequest("Id must be 0!");

                if (groupProfileDto.DateDeleted != null)
                    return Forbid("Cannot delete group profile!");

                var userProfile = await dbc.UserProfiles.FirstOrDefaultAsync(x => x.Identity == UserIdentity);
                if (userProfile == null)
                    return NotFound("User profile not found.");

                if (groupProfileDto.CreatorId != userProfile.Id)
                    return Forbid("CreatorId does not match!");

                var dateCreated = DateTime.UtcNow;
                groupProfileDto.DateCreated = dateCreated;
                var groupProfile = _mapper.Map<GroupProfile>(groupProfileDto);

                var chatRoom = new ChatRoom
                {
                    Type = ChatRoomTypeEnum.Group,
                    GroupProfile = groupProfile,
                    DateCreated = dateCreated,
                };

                var userChatRoom = new UserChatRoom
                {
                    UserProfileId = userProfile.Id,
                    ChatRoom = chatRoom,
                    UserRole = UserRoleEnum.FullNode | UserRoleEnum.GroupAdmin,
                    DateAdded = dateCreated,
                };

                dbc.UserChatRooms.Add(userChatRoom);
                await dbc.SaveChangesAsync();

                var userChatRoomDto = _mapper.Map<UserChatRoomDTO>(userChatRoom);
                return CreatedAtAction(nameof(GetGroupProfile), new { id = groupProfile.Id }, userChatRoomDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [HttpPost]
        [Route(nameof(JoinGroup))]
        public async Task<ActionResult<UserChatRoomDTO>> JoinGroup(long userProfileId, long groupProfileId, string joinToken)
        {
            try
            {
                if (string.IsNullOrEmpty(joinToken) || joinToken.Length > 63)
                    return BadRequest("Invalid token length.");

                var chatRoom = await dbc.ChatRooms
                                        .Include(x => x.GroupProfile)
                                        .FirstOrDefaultAsync(x => x.GroupProfileId == groupProfileId);

                if (chatRoom == null)
                    return NotFound("Group's chat room not found.");

                if (chatRoom.GroupProfile.JoinToken != joinToken)
                    return Forbid("Invalid token value!");

                var userProfile = dbc.UserProfiles.Find(userProfileId);

                if (userProfile == null)
                    return NotFound("User profile not found.");

                if (userProfile.Identity != UserIdentity)
                    return Forbid("User profile does not match!");

                var userChatRoom = await dbc.UserChatRooms
                                            .Where(x => x.UserProfileId == userProfileId && x.ChatRoomId == chatRoom.Id)
                                            .FirstOrDefaultAsync();
                if (userChatRoom != null)
                    return Conflict("User already joined to group.");

                userChatRoom = new UserChatRoom
                {
                    UserProfileId = userProfileId,
                    ChatRoomId = chatRoom.Id,
                    DateAdded = DateTime.UtcNow,
                };

                dbc.UserChatRooms.Add(userChatRoom);
                await dbc.SaveChangesAsync();

                var userChatRoomDto = _mapper.Map<UserChatRoomDTO>(userChatRoom);

                return CreatedAtRoute(
                    new
                    {
                        controller = "ChatRoom",
                        action = nameof(ChatRoomController.GetUser),
                        userChatRoomId = userChatRoomDto.Id
                    },
                    userChatRoomDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
