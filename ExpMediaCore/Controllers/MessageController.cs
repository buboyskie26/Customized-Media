﻿using ExpMedia.Application.Helper;
using ExpMedia.Application.MessageFolder;
using ExpMedia.Application.MessageGroupFolder;
using ExpMedia.Domain;
using ExpMedia.Persistence;
using ExpMediaCore.Repository.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMessage _message;

        public MessageController(DataContext context, UserManager<AppUser> userManager,
            IMessage message)
        {
            _context = context;
            _userManager = userManager;
            _message = message;
        }

        // Note: User could sent a message if she/he had followed the user.
        // If the user already sent a message to anyone, the user who had sent him
        // SHOULD use this route for reply/create more message between each other.
        [HttpPost("messageConversation")]
        public async Task<ActionResult> MessageActivity([FromBody] MessageCreationDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);

            var doesMyTableIds = await _message.PostMessageActivity(user.Id, dto);
            if (doesMyTableIds == true)
            {
                return Ok("Message sent.");
            }
            else
            {
                return BadRequest("Wrong Table Id.");
            }

            return Ok("Invalid Message Request.");
        }

        // Once the user clicks another user, it automatically triggered the posts route
        // Immediately goes to the /api/Message/messageFrom/{messageTableId} route
        [HttpPost("createMessageTo")]
        public async Task<ActionResult> CreateMessageTo([FromBody] MessageTableCreationDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);

            // Check if the user had already a table to that specific user which he wanted to communicate/chat.
            if (dto.MessageToId == user.Id)
            {
                return BadRequest("Chatting yourself is invalid. Chat other instead.");
            }

            var (validateMessageToUser,
                userSelectedToTheOriginalTable) = await _message.PostCreateMessageTo(user.Id, dto);

            if (validateMessageToUser == true)
            {
                if (userSelectedToTheOriginalTable == true)
                {
                    return Ok("Message Table Created.");
                }
                else
                {
                    return BadRequest("You already had a table.");
                }
                return BadRequest("Invalid userId selected");
            }

            return BadRequest("Invalid users selected");
        }

        [HttpGet("messageFrom/{messageTableId}")]
        public async Task<ActionResult<List<MessageDTO>>> MessageFromMessagTableType(int messageTableId)
        {
            var user = await _userManager.GetUserAsync(User);
            return await _message.GetMessageFrom(user.Id, messageTableId);
        }

        // V2
        [HttpGet("messageFrom2/{chatUserId}")]
        public async Task<ActionResult<List<MessageDTO>>> MessageFromUserIdTyoe(string chatUserId)
        {
            var user = await _userManager.GetUserAsync(User);

            return await _message.GetMessageFromUserId(user.Id, chatUserId);
        }
        // All users in here is the view of who`s the potential users gonna chat to
        // The /api/Message/createMessageTo route is the route where the id of users is the parameter to consume. 
        [HttpGet("allUsers")]
        public async Task<ActionResult<List<MessageSingleUsers>>> MessageView()
        {
            var user = await _userManager.GetUserAsync(User);
            return await _message.GetMessageView(user.Id);
        }


        // This root is the birds eye view in the message inbox
        // One to One Message. ----
        // If the user wanted to message single user, themy friends object has an userId which will be consumed 
        // by the *createMessageTo* posts route, where the route immediately create a table that serves as an chat id

        // Group Message. ----
        // We are assuming that the user has a group Id together with other user.
        // When the user wanted to enter to the chat room id, the MyGroupChats Object has a messagesGroupId which
        // will be consumed by /api/Message/messageToGroupPostGet, that would viewed the group messages.
        [HttpGet("userIHadFollowed")]
        public async Task<ActionResult<InboxView>> UserIHadFollowed([FromQuery] FilterFollowingsDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);

            return await _message.GetUserIHadFollowed(user.Id, dto);
        }

        // PostGet should be a users list that he had followed. Above HttpGet would be the route to be consumed.
        [HttpPost("createGroupMessage")]
        public async Task<ActionResult> CreateGroupMessage([FromForm] SubMessageGroupCreationDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);

            var myGroupChatIds = await _message.CheckMyGroupChatId(user.Id);

            if (dto.MessageToUserId.Contains(user.Id))
            {
                return BadRequest("You dont need to include yourself in the group..");

            }

            // If the same user who had created Group with the SAME other users would should be avoided.
            // Order by to sort, in order to be aligned and avoid being jumbled which resulted as conflict by its order.
            if (myGroupChatIds.SequenceEqual(dto.MessageToUserId.OrderBy(w => w))
                && myGroupChatIds.Count() == dto.MessageToUserId.Count())
            {
                return BadRequest("You have already created a group with the same users.");
            }

            /*var con = otherIds.Concat(myId).ToList();*/

            // Selecting a user must be two or more
            if (dto.MessageToUserId.Count > 1)
            {

                var group = await _message.PostCreateGroupMessage(user.Id, dto);
                return Ok($"Group '{group.GroupName}' has been created.");
            }
            else
            {
                return BadRequest("Two or more users needed for group chat.");
            }

            return Ok("Hmm");
        }
        // Serves as message inbox where user could sent a message in the chat box
        // Could see other user reply in that group chat.
        [HttpGet("messageToGroupPostGet")]
        public async Task<ActionResult<MessageToGroupPostGetView>> MessageToGroupPostGet(int messagesGroupsId)
        {
            var user = await _userManager.GetUserAsync(User);
            return await _message.GetMessageToGroupPostGet(user.Id, messagesGroupsId);
        }

        // Needs to have a POSTGET where users reflects his own SubMessageGroupId
        // That are registered in SubMessageGroups Table
        [HttpPost("messageToGroup")]
        public async Task<ActionResult> MessageToGroup([FromForm] MessageToGroupCreation dto)
        {
            var user = await _userManager.GetUserAsync(User);

            var results = await _message.PostMessageToGroup(user.Id, dto);


            if (results == true)
            {

                return Ok("Successful sent a message");

            }
            else
            {
                return BadRequest("Wrong Message Id");

            }

            return NotFound();
        }
        // Deleting my message to group chat.
        [HttpDelete("{subUserMessageId}")]
        public async Task<ActionResult> DeleteMessage(int subUserMessageId)
        {
            var user = await _userManager.GetUserAsync(User);

            var messageObj = await _context.SubUserMessages
                .Include(w=> w.SubMessageGroup)
                .Where(w=> w.SubMessageGroup.MessageToUserId == user.Id)
                .FirstOrDefaultAsync(w => w.Id == subUserMessageId);

            if (messageObj != null)
            {
                _context.SubUserMessages.Remove(messageObj);
                await _context.SaveChangesAsync();

                return Ok("Message Removed");
            }

            return NotFound();
        }

        [HttpDelete("{subMessageGroupId}")]
        public async Task<ActionResult> LeftTheGroup(int subMessageGroupId)
        {
            var user = await _userManager.GetUserAsync(User);

            var subMessage = await _context.SubMessageGroups
                .Where(w => w.MessageToUserId == user.Id)
                .FirstOrDefaultAsync(w => w.Id == subMessageGroupId);

            if (subMessage != null)
            {
                _context.SubMessageGroups.Remove(subMessage);
                await _context.SaveChangesAsync();

                return Ok("You Left the group.");
            }
            return NotFound();
        }

        // Group Chat Users other could add other users.
        [HttpPost("{subMessageGroupId}")]
        public async Task<ActionResult> AddOtherUsersInTheGroup(int subMessageGroupId)
        {
            var user = await _userManager.GetUserAsync(User);

            var subMessage = await _context.SubMessageGroups
                .Where(w => w.MessageToUserId == user.Id)
                .FirstOrDefaultAsync(w => w.Id == subMessageGroupId);

            if (subMessage != null)
            {
                _context.SubMessageGroups.Remove(subMessage);
                await _context.SaveChangesAsync();

                return Ok("You Left the group.");
            }
            return NotFound();
        }
    }
}
