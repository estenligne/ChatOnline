using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebAPI.Models;
using Global.Models;
using Global.Enums;
using AutoMapper;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    public class MessageController : BaseController<MessageController>
    {
        private readonly PushNotificationService _pushNotificationService;

        public MessageController(
            PushNotificationService pushNotificationService,
            ApplicationDbContext context,
            ILogger<MessageController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
            _pushNotificationService = pushNotificationService;
        }

        private MessageSentDTO GetMessage(MessageSent messageSent, MessageReceived messageReceived)
        {
            // First check whether the caller did the necessary .Include() for extra data:

            if (messageSent.MessageTag == null) // MessageTag is needed by app
                _logger.LogError("In GetMessage(): messageSent.MessageTag == null");

            if (messageSent.File == null && messageSent.FileId != null) // File is needed by app
                _logger.LogError("In GetMessage(): messageSent.File == null");

            if (messageSent.Sender?.UserProfile == null) // UserProfile needed below
                _logger.LogError("In GetMessage(): messageSent.Sender?.UserProfile == null");

            var messageSentDto = _mapper.Map<MessageSentDTO>(messageSent);
            messageSentDto.SenderName = messageSent.Sender?.UserProfile?.Username;

            if (messageReceived != null)
            {
                if (messageReceived.MessageSentId != messageSent.Id)
                    _logger.LogError($"In GetMessage(): {messageReceived.MessageSentId} != {messageSent.Id}");

                if (messageReceived.ReceiverId == messageSent.SenderId) // if user received their own message
                    _logger.LogError($"In GetMessage(): {messageReceived.ReceiverId} == {messageSent.SenderId}");

                messageSentDto.ReceiverId = messageReceived.ReceiverId;
                messageSentDto.DateReceived = messageReceived.DateReceived;
                messageSentDto.DateDeleted = messageReceived.DateDeleted;
                messageSentDto.DateStarred = messageReceived.DateStarred;
                messageSentDto.Reaction = messageReceived.Reaction;
            }

            if (messageSentDto.DateDeleted != null) // if a deleted message
            {
                messageSentDto.Body = null; // then reduce payload size a bit!
                messageSentDto.File = null; // reduce payload size some more!
            }
            return messageSentDto;
        }

        [HttpGet]
        [Route(nameof(GetMany))]
        public async Task<ActionResult<List<MessageSentDTO>>> GetMany(long userChatRoomId)
        {
            try
            {
                var userChatRoom = await dbc.UserChatRooms
                                            .Include(x => x.UserProfile)
                                            .FirstOrDefaultAsync(x => x.Id == userChatRoomId);

                if (userChatRoom == null)
                    return NotFound("Sender's user chat room not found.");

                if (userChatRoom.UserProfile.Identity != UserIdentity)
                    return Forbid("Sender's user chat room does not match!");

                if (userChatRoom.DateBlocked != null ||
                    userChatRoom.DateDeleted != null ||
                    userChatRoom.DateExited != null)
                    return Forbid("You are not allowed to view messages in this chat room!");

                var messagesSent = await dbc.MessagesSent
                                            .Include(x => x.Sender.UserProfile)
                                            .Include(x => x.MessageTag)
                                            .Include(x => x.File)
                                            .Where(x => x.Sender.ChatRoomId == userChatRoom.ChatRoomId)
                                            .OrderBy(x => x.Id)
                                            .ToListAsync();

                var messageSentIds = messagesSent.Select(x => x.Id);

                var messagesReceived = await dbc.MessagesReceived
                                                .Where(x => x.ReceiverId == userChatRoomId &&
                                                    messageSentIds.Contains(x.MessageSentId))
                                                .ToDictionaryAsync(x => x.MessageSentId);

                var utcNow = DateTime.UtcNow;
                var messages = new List<MessageSentDTO>();

                foreach (var messageSent in messagesSent)
                {
                    MessageReceived messageReceived = null;
                    messagesReceived.TryGetValue(messageSent.Id, out messageReceived);

                    if (messageReceived == null && messageSent.SenderId != userChatRoomId)
                    {
                        _logger.LogWarning($"GetMany({userChatRoomId}): user did not receive message {messageSent.Id}.");
                        messageReceived = await AddMessageReceived(messageSent, userChatRoom.Id, utcNow);
                    }

                    messages.Add(GetMessage(messageSent, messageReceived));
                }
                return messages;
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static bool LesserTime(DateTime a, DateTime b)
        {
            return a < b + TimeSpan.FromSeconds(30);
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [HttpPost]
        public async Task<ActionResult<MessageSentDTO>> PostMessage(MessageSentDTO messageSentDto)
        {
            try
            {
                var validationResults = ValidateObject(messageSentDto);
                if (validationResults != null)
                    return BadRequest(validationResults);

                if (messageSentDto.Id != 0)
                    return BadRequest("Id must be 0!");

                if (messageSentDto.DateDeleted != null)
                    return Forbid("Cannot delete this message!");

                if (messageSentDto.AuthorId != null && messageSentDto.LinkedId == null)
                    return BadRequest("No linked message provided for the forwarded message.");

                var dateCreated = DateTime.UtcNow;
                if (!LesserTime(messageSentDto.DateSent, dateCreated))
                    return BadRequest("The date sent cannot be in the future!");

                var userChatRoom = await dbc.UserChatRooms
                                            .Include(x => x.UserProfile)
                                            .Include(x => x.ChatRoom.GroupProfile)
                                            .Where(x => x.Id == messageSentDto.SenderId)
                                            .FirstOrDefaultAsync();

                if (userChatRoom == null)
                    return NotFound("Sender's user chat room not found.");

                if (userChatRoom.UserProfile.Identity != UserIdentity)
                    return Forbid("Sender's user chat room does not match!");

                if (messageSentDto.LinkedId != null)
                {
                    var linked = dbc.MessagesSent.Find(messageSentDto.LinkedId);
                    if (linked == null)
                        return NotFound("Linked message not found.");

                    if (linked.AuthorId == null)
                    {
                        var linkedSender = dbc.UserChatRooms.Find(linked.SenderId);

                        if (messageSentDto.AuthorId != null && messageSentDto.AuthorId != linkedSender.UserProfileId)
                            return Forbid("The author of this forwarded message is incorrect!");
                    }
                    else
                    {
                        if (messageSentDto.AuthorId != null && messageSentDto.AuthorId != linked.AuthorId)
                            return Forbid("Cannot change the author when forwarding a message!");

                        if (messageSentDto.MessageType != linked.MessageType)
                            return Forbid("Cannot change the message type of a forwarded message!");

                        if (messageSentDto.Body != linked.Body)
                            return Forbid("Cannot change the message body of a forwarded message!");

                        if (messageSentDto.FileId != linked.FileId)
                            return Forbid("Cannot change the file associated to a forwarded message!");
                    }
                }

                var tag = messageSentDto.MessageTag;
                if (tag != null) // will always be true, here just as an excuse to indent!
                {
                    if (tag.Name == "")
                        tag.Name = null;

                    if (tag.ChatRoomId != userChatRoom.ChatRoomId)
                        return BadRequest($"tag.ChatRoomId {tag.ChatRoomId} != ChatRoomId {userChatRoom.ChatRoomId}");

                    var messageTag = await dbc.MessageTags
                                            .Where(x => x.Name == tag.Name && x.ChatRoomId == userChatRoom.ChatRoomId)
                                            .FirstOrDefaultAsync();

                    if (messageTag == null)
                    {
                        if (tag.Id != 0)
                            return BadRequest("The MessageTag.Id must be 0!");

                        messageTag = new MessageTag()
                        {
                            Name = tag.Name,
                            ChatRoomId = userChatRoom.ChatRoomId,
                            CreatorId = userChatRoom.UserProfileId,
                            DateCreated = dateCreated,
                        };

                        dbc.MessageTags.Add(messageTag);
                        await dbc.SaveChangesAsync(); // get messageTag.Id
                    }
                    else if (tag.Id != 0 && tag.Id != messageTag.Id)
                        return Forbid("The MessageTag.Id is not valid!");

                    tag.Id = messageTag.Id;
                }

                messageSentDto.MessageTagId = tag.Id; // prepare for mapper
                messageSentDto.MessageTag = null; // do not update database data
                messageSentDto.File = null; // do not update file database data

                var messageSent = _mapper.Map<MessageSent>(messageSentDto);
                messageSent.DateReceicedByServer = dateCreated;

                dbc.MessagesSent.Add(messageSent);
                await dbc.SaveChangesAsync();

                string title = userChatRoom.UserProfile.Username;
                string group = userChatRoom.ChatRoom.GroupProfile?.GroupName;
                if (group != null)
                    title += " - " + group;

                // Send the MessageSent push notification
                var pushNotificationDto = new PushNotificationDTO
                {
                    Topic = PushNotificationTopic.MessageSent,
                    DateCreated = dateCreated,
                    UserChatRoomId = userChatRoom.Id,
                    MessageSentId = messageSent.Id,

                    Id = (int)DateTime.UtcNow.Ticks,
                    Title = title,
                    Body = messageSentDto.Body,
                    Priority = true,
                };

                var userProfileIds = await dbc.UserChatRooms
                                            .Where(x => x.Id != userChatRoom.Id &&
                                                x.ChatRoomId == userChatRoom.ChatRoomId &&
                                                x.DateBlocked == null &&
                                                x.DateDeleted == null &&
                                                x.DateExited == null)
                                            .Select(x => x.UserProfileId)
                                            .ToListAsync();

                var outcomes = await _pushNotificationService.SendAsync(dbc, userProfileIds, pushNotificationDto);

                messageSentDto.SenderName = userChatRoom.UserProfile.Username;
                messageSentDto.Id = messageSent.Id;
                return CreatedAtAction(nameof(GetMany), null, messageSentDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [HttpPost]
        [Route(nameof(Received))]
        public async Task<ActionResult<MessageSentDTO>> Received(long messageSentId, DateTime dateReceived)
        {
            try
            {
                var messageSent = await dbc.MessagesSent
                                            .Include(x => x.Sender.UserProfile)
                                            .Include(x => x.MessageTag)
                                            .Include(x => x.File)
                                            .FirstOrDefaultAsync(x => x.Id == messageSentId);

                if (messageSent == null)
                    return NotFound($"messageSentId {messageSentId} not found.");

                if (!LesserTime(messageSent.DateReceicedByServer, dateReceived))
                    return BadRequest("Date received cannot be before date sent!");

                if (!LesserTime(dateReceived, DateTime.UtcNow))
                    return BadRequest("Date received cannot be in the future!");

                var userChatRoom = await dbc.UserChatRooms
                                            .Where(x => x.ChatRoomId == messageSent.Sender.ChatRoomId
                                                && x.UserProfile.Identity == UserIdentity)
                                            .FirstOrDefaultAsync();

                if (userChatRoom == null)
                    return NotFound($"User chat room not found.");

                if (messageSent.SenderId == userChatRoom.Id)
                    return BadRequest("Cannot receive your own message sent!");

                var messageReceived = await dbc.MessagesReceived
                                            .Where(x => x.ReceiverId == userChatRoom.Id && x.MessageSentId == messageSentId)
                                            .FirstOrDefaultAsync();

                if (messageReceived != null)
                    return Conflict("Message already received.");

                messageReceived = await AddMessageReceived(messageSent, userChatRoom.Id, dateReceived);

                var messageSentDto = GetMessage(messageSent, messageReceived);
                return CreatedAtAction(nameof(GetMany), null, messageSentDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private async Task<MessageReceived> AddMessageReceived(MessageSent messageSent, long userChatRoomId, DateTime dateReceived)
        {
            var messageReceived = new MessageReceived()
            {
                ReceiverId = userChatRoomId,
                MessageSentId = messageSent.Id,
                DateReceived = dateReceived,
            };

            dbc.MessagesReceived.Add(messageReceived);
            await dbc.SaveChangesAsync();

            // Send the MessageReceived push notification
            var pushNotificationDto = new PushNotificationDTO
            {
                Topic = PushNotificationTopic.MessageReceived,
                DateCreated = DateTime.UtcNow,
                UserChatRoomId = userChatRoomId,
                MessageSentId = messageSent.Id,
            };

            var userProfileIds = new List<long> { messageSent.Sender.UserProfileId };
            await _pushNotificationService.SendAsync(dbc, userProfileIds, pushNotificationDto);

            return messageReceived;
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPost]
        [Route(nameof(Read))]
        public async Task<ActionResult<MessageSentDTO>> Read(long messageSentId, DateTime dateRead)
        {
            try
            {
                var messageReceived = await dbc.MessagesReceived
                                        .Include(x => x.MessageSent.Sender)
                                        .Where(x => x.MessageSentId == messageSentId &&
                                            x.Receiver.UserProfile.Identity == UserIdentity)
                                        .FirstOrDefaultAsync();

                if (messageReceived == null)
                    return NotFound($"messageReceived.MessageSentId {messageSentId} not found.");

                if (!LesserTime(messageReceived.DateReceived, dateRead))
                    return BadRequest("Date read cannot be before date received!");

                if (!LesserTime(dateRead, DateTime.UtcNow))
                    return BadRequest("Date read cannot be in the future!");

                if (messageReceived.DateRead != null)
                    return Conflict("Message already read.");

                messageReceived.DateRead = dateRead;
                await dbc.SaveChangesAsync();

                // Send the MessageRead push notification
                var pushNotificationDto = new PushNotificationDTO
                {
                    Topic = PushNotificationTopic.MessageRead,
                    DateCreated = DateTime.UtcNow,
                    UserChatRoomId = messageReceived.ReceiverId,
                    MessageSentId = messageReceived.MessageSentId,
                };

                var userProfileIds = new List<long> { messageReceived.MessageSent.Sender.UserProfileId };
                var outcomes = await _pushNotificationService.SendAsync(dbc, userProfileIds, pushNotificationDto);

                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
