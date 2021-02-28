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

namespace WebAPI.Controllers
{
    public class MessageController : BaseController<MessageController>
    {
        public MessageController(
            ApplicationDbContext context,
            ILogger<MessageController> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        private MessageSentDTO GetMessage(MessageSent messageSent, MessageReceived messageReceived)
        {
            if (messageSent.MessageTag == null) // Simple check for .Include() in caller. Do NOT remove.
                _logger.LogError("In GetMessage(): messageSent.MessageTag == null");

            var messageSentDto = _mapper.Map<MessageSentDTO>(messageSent);

            if (messageReceived != null)
            {
                if (messageReceived.MessageSentId != messageSent.Id)
                    _logger.LogError($"In GetMessage(): {messageReceived.MessageSentId} != {messageSent.Id}");

                if (messageReceived.ReceiverId == messageSent.SenderId)
                    _logger.LogError($"In GetMessage(): {messageReceived.ReceiverId} == {messageSent.SenderId}");

                messageSentDto.DateReceived = messageReceived.DateReceived;
                messageSentDto.DateDeleted = messageReceived.DateDeleted;
                messageSentDto.DateStarred = messageReceived.DateStarred;
                messageSentDto.Reaction = messageReceived.Reaction;
            }

            if (messageSentDto.DateDeleted != null) // if a deleted message
            {
                messageSentDto.Body = null; // then save payload a bit!
                messageSentDto.File = null;
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
                        .Where(x => x.Id == userChatRoomId && x.UserProfile.User.UserName == User.Identity.Name)
                        .FirstOrDefaultAsync();

                if (userChatRoom == null)
                    return NotFound("Sender's user chat room not found.");

                var messagesSent = await dbc.MessagesSent
                                            .Include(x => x.MessageTag)
                                            .Where(x => x.MessageTag.ChatRoomId == userChatRoom.ChatRoomId)
                                            .ToListAsync();

                var messageSentIds = messagesSent.Select(x => x.Id);

                var messagesReceived = await dbc.MessagesReceived
                                                .Where(x => messageSentIds.Contains(x.MessageSentId) && x.ReceiverId == userChatRoom.Id)
                                                .ToDictionaryAsync(x => x.MessageSentId);

                var messages = new List<MessageSentDTO>();

                foreach (var messageSent in messagesSent)
                {
                    MessageReceived messageReceived = null;
                    messagesReceived.TryGetValue(messageSent.Id, out messageReceived);
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
                    return Unauthorized("Id does not match!");

                if (messageSentDto.DateDeleted != null)
                    return Unauthorized("Cannot delete this message!");

                if (messageSentDto.AuthorId != null && messageSentDto.LinkedId == null)
                    return BadRequest("No linked message provided for the forwarded message.");

                var dateCreated = DateTime.UtcNow;
                if (!LesserTime(messageSentDto.DateSent, dateCreated))
                    return BadRequest("The date sent cannot be in the future!");

                var userChatRoom = await dbc.UserChatRooms
                        .Where(x => x.Id == messageSentDto.SenderId && x.UserProfile.User.UserName == User.Identity.Name)
                        .FirstOrDefaultAsync();

                if (userChatRoom == null)
                    return NotFound("Sender's user chat room not found.");

                if (messageSentDto.LinkedId != null)
                {
                    var linked = dbc.MessagesSent.Find(messageSentDto.LinkedId);
                    if (linked == null)
                        return NotFound("Linked message not found.");

                    if (linked.AuthorId == null)
                    {
                        var linkedSender = dbc.UserChatRooms.Find(linked.SenderId);

                        if (messageSentDto.AuthorId != null && messageSentDto.AuthorId != linkedSender.UserProfileId)
                            return Unauthorized("The author of this forwarded message is incorrect!");
                    }
                    else
                    {
                        if (messageSentDto.AuthorId != null && messageSentDto.AuthorId != linked.AuthorId)
                            return Unauthorized("Cannot change the author when forwarding a message!");

                        if (messageSentDto.MessageType != linked.MessageType)
                            return Unauthorized("Cannot change the message type of a forwarded message!");

                        if (messageSentDto.Body != linked.Body)
                            return Unauthorized("Cannot change the message body of a forwarded message!");

                        if (messageSentDto.FileId != linked.FileId)
                            return Unauthorized("Cannot change the file associated to a forwarded message!");
                    }
                }

                var tag = messageSentDto.MessageTag;
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
                        return Unauthorized("The MessageTag.Id must be 0!");

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
                    return Unauthorized("The MessageTag.Id is not valid!");

                tag.Id = messageTag.Id;
                messageSentDto.MessageTagId = messageTag.Id; // prepare for mapper
                messageSentDto.MessageTag = null; // do not update database data
                messageSentDto.File = null; // do not update file database data

                var messageSent = _mapper.Map<MessageSent>(messageSentDto);
                messageSent.DateReceicedByServer = dateCreated;

                dbc.MessagesSent.Add(messageSent);
                await dbc.SaveChangesAsync();

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
                                            .Include(x => x.Sender)
                                            .Include(x => x.MessageTag)
                                            .FirstOrDefaultAsync(x => x.Id == messageSentId);
                if (messageSent == null)
                    return NotFound($"messageSentId {messageSentId} not found.");

                if (!LesserTime(messageSent.DateReceicedByServer, dateReceived))
                    return BadRequest("Date received cannot be before date sent!");

                if (!LesserTime(dateReceived, DateTime.UtcNow))
                    return BadRequest("Date received cannot be in the future!");

                var userChatRoom = await dbc.UserChatRooms
                                            .Where(x => x.ChatRoomId == messageSent.Sender.ChatRoomId
                                                && x.UserProfile.User.UserName == User.Identity.Name)
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

                messageReceived = new MessageReceived()
                {
                    ReceiverId = userChatRoom.Id,
                    MessageSentId = messageSentId,
                    DateReceived = dateReceived,
                };

                dbc.MessagesReceived.Add(messageReceived);
                await dbc.SaveChangesAsync();

                var messageSentDto = GetMessage(messageSent, messageReceived);
                return CreatedAtAction(nameof(GetMany), null, messageSentDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPost]
        [Route(nameof(Read))]
        public async Task<ActionResult<MessageSentDTO>> Read(long messageReceivedId, DateTime dateRead)
        {
            try
            {
                var messageReceived = await dbc.MessagesReceived
                                        .Where(x => x.Id == messageReceivedId &&
                                            x.Receiver.UserProfile.User.UserName == User.Identity.Name)
                                        .FirstOrDefaultAsync();

                if (messageReceived == null)
                    return NotFound($"messageReceivedId {messageReceivedId} not found.");

                if (!LesserTime(messageReceived.DateReceived, dateRead))
                    return BadRequest("Date read cannot be before date received!");

                if (!LesserTime(dateRead, DateTime.UtcNow))
                    return BadRequest("Date read cannot be in the future!");

                messageReceived.DateRead = dateRead;
                await dbc.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
