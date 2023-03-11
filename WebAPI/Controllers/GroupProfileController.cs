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

        private Task<UserRoom> GetUserChatRoom(long id)
        {
            return dbc.UserRooms
                    .Where(x => x.UserId == UserId && x.RoomId == id)
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

                var groupProfile = await dbc.Groups
                                            .Include(x => x.File)
                                            .Include(x => x.WallpaperFile)
                                            .FirstOrDefaultAsync(x => x.RoomId == id);

                var groupProfileDto = _mapper.Map<GroupProfileDTO>(groupProfile);

                if ((userChatRoom.Role & UserRoleEnum.GroupAdmin) == 0)
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

                var chatRoom = dbc.Rooms.Find(id);
                if (chatRoom.CreatorId != userChatRoom.UserId)
                    return Forbid("Cannot delete group profile!");

                chatRoom.DateDeleted = DateTimeOffset.UtcNow;
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

                long id = groupProfileDto.ChatRoomId;

                var userChatRoom = await GetUserChatRoom(id);
                if (userChatRoom == null)
                    return NotFound("Group profile not found.");

                if ((userChatRoom.Role & UserRoleEnum.GroupAdmin) == 0)
                    return Forbid("Cannot update group profile!");

                var chatRoom = dbc.Rooms.Find(id);

                if (chatRoom.CreatorId != userChatRoom.UserId)
                    return Forbid("CreatorId does not match!");

                var groupProfile = dbc.Groups.Find(id);

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

                if (groupProfileDto.ChatRoomId != 0)
                    return BadRequest("Id must be 0!");

                var dateCreated = DateTimeOffset.UtcNow;
                UserRoom userChatRoom = null;

                Room chatRoom = await dbc.Rooms
                    .Include(c => c.GroupProfile)
                    .Where(c => c.CreatorId == UserId &&
                        c.GroupProfile.Name == groupProfileDto.GroupName)
                    .FirstOrDefaultAsync();

                Group groupProfile = chatRoom?.GroupProfile;

                if (groupProfile == null)
                {
                    groupProfile = _mapper.Map<Group>(groupProfileDto);
                    chatRoom = new Room
                    {
                        Type = ChatRoomTypeEnum.Group,
                        GroupProfile = groupProfile,
                        DateCreated = dateCreated,
                        CreatorId = UserId,
                    };
                }
                else if (chatRoom.DateDeleted != null) // if was deleted
                {
                    chatRoom.DateDeleted = null;
                    groupProfileDto.ChatRoomId = groupProfile.RoomId;
                    _mapper.Map(groupProfileDto, groupProfile);
                    userChatRoom = await GetUserChatRoom(chatRoom.Id);
                }
                else return Conflict("Cannot create another group with the same name!");

                if (userChatRoom == null)
                {
                    userChatRoom = new UserRoom
                    {
                        UserId = UserId,
                        Room = chatRoom,
                        Role = UserRoleEnum.FullNode | UserRoleEnum.GroupAdmin,
                        DateAdded = dateCreated,
                    };
                    dbc.UserRooms.Add(userChatRoom);
                }
                else
                {
                    userChatRoom.DateDeleted = null;
                    userChatRoom.DateExited = null;
                    userChatRoom.DateBlocked = null;
                }
                await dbc.SaveChangesAsync();

                var userChatRoomDto = _mapper.Map<UserChatRoomDTO>(userChatRoom);
                return CreatedAtAction(nameof(GetGroupProfile), new { id = chatRoom.Id }, userChatRoomDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [HttpPost]
        [Route(nameof(JoinGroup))]
        public async Task<ActionResult<UserChatRoomDTO>> JoinGroup(long id, string joinToken)
        {
            try
            {
                if (string.IsNullOrEmpty(joinToken) || joinToken.Length > 63)
                    return BadRequest("Invalid token length.");

                var chatRoom = await dbc.Rooms
                                        .Include(x => x.GroupProfile)
                                        .FirstOrDefaultAsync(x => x.Id == id);

                if (chatRoom == null)
                    return NotFound($"Group {id} not found.");

                if (chatRoom.GroupProfile.JoinToken != joinToken)
                    return Forbid("Invalid token value!");

                var userChatRoom = await GetUserChatRoom(id);
                if (userChatRoom != null)
                    return Conflict("User already joined to group.");

                userChatRoom = new UserRoom
                {
                    UserId = UserId,
                    RoomId = id,
                    DateAdded = DateTimeOffset.UtcNow,
                };

                dbc.UserRooms.Add(userChatRoom);
                await dbc.SaveChangesAsync();

                var userChatRoomDto = _mapper.Map<UserChatRoomDTO>(userChatRoom);

                return CreatedAtRoute(
                    new
                    {
                        controller = "Room",
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
