using ExpMedia.Application.Helper;
using ExpMedia.Application.MessageFolder;
using ExpMedia.Application.MessageGroupFolder;
using ExpMedia.Domain;
using ExpMedia.Persistence;
using ExpMediaCore.Repository.IService;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.Repository.Service
{
    public class MessageRepository : IMessage
    {
        private readonly DataContext _context;

        public MessageRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Tuple<bool, bool>> PostCreateMessageTo(string userId, MessageTableCreationDTO dto)
        {
            bool validateMessageToUser = await DoesUserExists(dto);

            // User who are selected by the user who had made the original table.
            bool userSelectedToTheOriginalTable = await UserSelectedOrUserMadeTable(userId, dto);

            // Use who made the original table.
            var userMadeOriginalTable = await _context.MessageTables
                .Where(w => w.MessageById == userId)
                .Where(w => w.MessageToUserId == dto.MessageToId)
                .AnyAsync();
            // User SHOULD`NT` Message his own. ( Own preference. )


            // bug what if other user message the same user in the table db.
            // Check if the user had already a table to that specific user which he wanted to communicate/chat.
            if (dto.MessageToId == userId)
            {
                /*return BadRequest("Chatting yourself is invalid. Chat other instead.");*/
            }
            if (validateMessageToUser == true)
            {
                /*if (userMadeOriginalTable == false && userSelectedToTheOriginalTable == false)*/
                // Use who made the original table.
                // User who are selected by the user who had made the original table.
                if (userSelectedToTheOriginalTable == true)
                {

                    var msg = new MessageTable()
                    {
                        MessageById = userId,
                        MessageToUserId = dto.MessageToId,
                        Messagesx = BuildMessage(dto.MessageToId, userId, dto.Body)
                    };

                    await _context.MessageTables.AddAsync(msg);
                    await _context.SaveChangesAsync();

                }
            }
            var values = new Tuple<bool, bool>(validateMessageToUser, userSelectedToTheOriginalTable);
            return await Task.FromResult(values);
        }
        private async Task<bool> DoesUserExists(MessageTableCreationDTO dto)
        {
            return await _context.AppUser
              .AnyAsync(w => w.Id == dto.MessageToId);
        }
        private async Task<bool> UserSelectedOrUserMadeTable(string userId, MessageTableCreationDTO dto)
        {
            // Use who made the original table.
            var userWhoMadeTable  = await _context.MessageTables
                .Where(w => w.MessageById == userId)
                .Where(w => w.MessageToUserId == dto.MessageToId)
                .AnyAsync();
            // User who are selected by the user who had made the original table.
            var userSelected= await _context.MessageTables
                .Where(w => w.MessageToUserId == userId)
                .Where(w => w.MessageById == dto.MessageToId)
                .AnyAsync();
        
            return userWhoMadeTable == false && userSelected == false;
        }
        private List<Messages> BuildMessage(string messageToId, string userId, string body)
        {
            var r = new List<Messages>();
            if (string.IsNullOrEmpty(messageToId) == false)
            {
                r.Add(new()
                {
                    /*MessageToUserId = messageToId,*/
                    MessageCreated = DateTime.Now,
                    MessageById = userId,
                    Body = body
                });
            }
            return r;
        }
        public async Task<bool> PostMessageActivity(string userId, MessageCreationDTO dto)
        {
            
            // Check the tableId of the user whom wanted to chat the single user

            var doesMyTableId = await _context.MessageTables
                .Where(w => w.MessageToUserId == userId || w.MessageById == userId)
                .AnyAsync(w => w.Id == dto.MessageTableId);

            if (doesMyTableId == true)
            {
                var msg = new Messages()
                {
                    MessageCreated = DateTime.Now,
                   /* MessageToUserId = dto.MessageToId,*/
                    MessageById = userId,
                    Body = dto.Body,
                    MessageTableId = dto.MessageTableId
                };

                await _context.Messagesx.AddAsync(msg);
                await _context.SaveChangesAsync();
            }
            return doesMyTableId;
        }
        public async Task<List<MessageDTO>> GetMessageFrom(string userId, int messageTableId)
        {
            var messageTo = _context.MessageTables
               .Select(w => w.MessageToUserId).ToList();

            var messageBy = _context.MessageTables
             .Select(w => w.MessageById).ToList();

            var twoUserConversation = messageTo.Concat(messageBy).Distinct().ToList();

            // All of your replies and users messages on you could see only by you and the one who had message you.

            var item = await (from p in _context.Messagesx

                              where p.MessageTableId == messageTableId
                              where p.MessageTable.MessageToUserId == userId || p.MessageTable.MessageById == userId
 
                              select new MessageDTO()
                              {
                                  MessageFromId = p.MessageById,
                                  Body = p.Body,
                                  MessageFromUsername = p.MessageBy.FirstName + " " + p.MessageBy.LastName,
                                  DateMessage = p.MessageCreated,
                                  MessageId = p.Id,
                                  MessageTableId = p.MessageTableId
                              }).OrderBy(w => w.DateMessage).AsNoTracking().ToListAsync();
            return item;
        }
        public async Task<List<MessageSingleUsers>> GetMessageView(string userId)
        {
            var msg = _context.MessageTables
               .Where(w => w.MessageById == userId)
               .Select(w => w.MessageToUserId)
               .ToList();

            var allUsers = await (from p in _context.AppUser
                                      /*join py in _context.MessageTables on p.Id equals py.MessageToUserId*/
                                  where p.Id != userId

                                  select new MessageSingleUsers()
                                  {
                                      ImageUrl = p.ImageUrl,
                                      UserId = p.Id,
                                      Username = p.FirstName + " " + p.LastName,
                                      MessageTableId = 0
                                  }).AsNoTracking().ToListAsync();

            var userWhoHadMessageById = await (from p in _context.AppUser
                                               join py in _context.MessageTables on p.Id equals py.MessageById
                                               where p.Id != userId
                                               select new MessageSingleUsers()
                                               {
                                                   ImageUrl = p.ImageUrl,
                                                   UserId = p.Id,
                                                   Username = p.FirstName + " " + p.LastName,
                                                   MessageTableId = py.Id
                                               }).AsNoTracking().ToListAsync();

            // All username needs to be unique.
            userWhoHadMessageById = userWhoHadMessageById.GroupBy(w => w.Username).Select(w => w.First()).ToList();

            var userWhoHadMessageToUserId = await (from p in _context.AppUser
                                                   join py in _context.MessageTables on p.Id equals py.MessageToUserId
                                                   /*where p.Id != userId*/
                                                   select new MessageSingleUsers()
                                                   {
                                                       ImageUrl = p.ImageUrl,
                                                       UserId = p.Id,
                                                       Username = p.FirstName + " " + p.LastName,
                                                       MessageTableId = py.Id
                                                   }).AsNoTracking().ToListAsync();
            // Joining the two`s userId wether you start and made the message or other had messaged you.
            var joinUserWhoMadeChatAndUserWhoChosen = userWhoHadMessageById.Concat(userWhoHadMessageToUserId).ToList();
            // Select the Username of the joined table.
            var joingUUsername = joinUserWhoMadeChatAndUserWhoChosen.Select(w => w.Username).ToList();
            // Filter all the same username from allUsers table that depends on the username in the joinUserWhoMadeChatAndUserWhoChosen username
            var appUser = allUsers.Where(w => joingUUsername.Contains(w.Username) == false).ToList();
            // As the appUser filterized, all joinUserWhoMadeChatAndUserWhoChosen obj which having a messageTableid
            // would be added together with the appUser whose didnt have any messageTableId.
            var joiningUserHadTableIdAndUserHadnt = joinUserWhoMadeChatAndUserWhoChosen.Concat(appUser).ToList();

            return joiningUserHadTableIdAndUserHadnt.OrderByDescending(w => w.MessageTableId).ToList();
        }

        public async Task<InboxView> GetUserIHadFollowed(string userId, FilterFollowingsDTO dto)
        {
            var userFollowingQuery = _context.UserFollowings.AsQueryable();

            if (string.IsNullOrEmpty(dto.Username) == false)
            {
                userFollowingQuery = GetUsername(dto, userFollowingQuery);
            }
            // Once the user clicks another user, it automatically triggered the *createMessageTo* posts route
            // Immediately goes to the /api/Message/messageFrom/{messageTableId} route
            var userFollowedList = await (from p in userFollowingQuery
                                          where p.UserWhoFollowedId == userId
                                          select new MessageUsers()
                                          {
                                              ImageUrl = p.UserToFollow.ImageUrl,
                                              UserId = p.UserToFollow.Id,
                                              Username = p.UserToFollow.FirstName + " " + p.UserToFollow.LastName
                                          }).Paginate(dto.PaginationDTO).OrderBy(w => w.Username)
                              .AsNoTracking().ToListAsync();

            // If the user has a group chat it will show to the inbox chat.
            var doesGroupIncludesMe = _context.SubMessageGroups
                .Where(w => w.MessageToUserId == userId)
                .Select(w => w.MessageToUserId).Distinct().ToList();

            var latestChat = _context.SubMessageGroups
                .Where(w => w.MessageToUserId == userId)
                .Select(w => w.MessagesGroupId)
                .ToList();

            // Get All the SubUserMessages who has the same GroupId to the latestChat.MessagesGroupId
            var groupMessages = _context.SubUserMessages
                .Include(w => w.SubMessageGroup).ThenInclude(w => w.SubUserMessagesx)
                .Include(w => w.SubMessageGroup).ThenInclude(w => w.MessagesGroup).ThenInclude(w => w.SubMessageGroups)
                .Where(w => latestChat.Contains(w.SubMessageGroup.MessagesGroupId))
                .OrderByDescending(w => w.MessageCreation)
                .ToList();

            var OuterGroupChatView = groupMessages.GroupBy(w => w.GroupId)
                .Select(w => w.First())
                .Select(t => new MyGroupChat()
                {
                    GroupName = t.SubMessageGroup.MessagesGroup.GroupName,
                    LatestChat = t.Body,
                    MessagesGroupId = t.SubMessageGroup.MessagesGroupId,
                    NumberOfUser = t.SubMessageGroup.SubUserMessagesx.Count()
                })
                .ToList();

            return new InboxView()
            {
                MyFriends = userFollowedList,
                MyGroupChats = OuterGroupChatView
            };
        }

        private static IQueryable<UserFollowing> GetUsername(FilterFollowingsDTO dto, IQueryable<UserFollowing> userFollowingQuery)
        {
            return userFollowingQuery.Where(w => w.UserToFollow.FirstName.ToLower()
                                .Contains(dto.Username.ToLower())
                                || w.UserToFollow.LastName.ToLower().Contains(dto.Username.ToLower()));
        }

        public async Task<List<MessageDTO>> GetMessageFromUserId(string userId, string chatUserId)
        {
            var messageTo = _context.MessageTables
              .Select(w => w.MessageToUserId).ToList();

            var messageBy = _context.MessageTables
             .Select(w => w.MessageById).ToList();

            var twoUserConversation = messageTo.Concat(messageBy).Distinct().ToList();


            // Get the userId whose you wanted to chat
            // Check if the chatuserId (parameter) that you`re trying to view the message,
            // Check if that chatUserId has been registered in the message table together with your id

            // If chatUser is null, it means there`s no id between that chatuserId and him
            // So it needs to create automatically and /api/Message/createMessageTo route would be trigger

            var chatUser = await _context.MessageTables
                .Where(w => (w.MessageById == chatUserId && w.MessageToUserId == userId)
                 || w.MessageToUserId == chatUserId && w.MessageById == userId)
                .FirstOrDefaultAsync();

            var oneToOneMessageList = new List<MessageDTO>();

            if (chatUser == null)
            {
                // If the loginUser clicks other user, if they had a message table, GET POST would be triggered
                // Else create a message table between the loginUser and the chatUserId (parameter)

                var msg = new MessageTable()
                {
                    MessageById = userId,
                    MessageToUserId = chatUserId,
                    Messagesx = BuildMessage(chatUserId, userId, string.Empty)
                };
                // At first if the chatUserId is null, this would create the object
                // The oneToOneMessageList list wont be null.
                await _context.MessageTables.AddAsync(msg);
                await _context.SaveChangesAsync();

            }
            // All of your replies and users messages on you could see only by you and the one who had message you.
            if (oneToOneMessageList != null)
            {
                oneToOneMessageList = await (from p in _context.Messagesx

                                                 /*where p.Body != string.Empty*/
                                             where (p.MessageTable.MessageById == chatUserId && p.MessageTable.MessageToUserId == userId)
                                               || (p.MessageTable.MessageToUserId == chatUserId && p.MessageTable.MessageById == userId)
                                             select new MessageDTO()
                                             {
                                                 MessageFromId = p.MessageById,
                                                 Body = p.Body,
                                                 MessageFromUsername = p.MessageBy.FirstName + " " + p.MessageBy.LastName,
                                                 DateMessage = p.MessageCreated,
                                                 MessageId = p.Id,
                                                 MessageTableId = p.MessageTableId
                                             }).OrderBy(w => w.DateMessage).AsNoTracking().ToListAsync();
            }
 

            return oneToOneMessageList;
        }

        public async Task<MessageToGroupPostGetView> GetMessageToGroupPostGet(string userId, int messagesGroupsId)
        {
            var groupMessageDetails = new List<SpecificGroupMessages>();
            var groupDetails = new MyGroupDetails();

            var useMadeTheGroup = _context.MessagesGroups
                .Where(w => w.Id == messagesGroupsId)
                .Select(w => w.UserMadeById).ToList();

            var groupBelongIds = _context.SubMessageGroups
                .Where(w => w.MessagesGroupId == messagesGroupsId)
                .Select(w => w.MessageToUserId).ToList();

            var allGroupMember = groupBelongIds.Concat(useMadeTheGroup).ToList();

            var ifIAmIncludedInTheGroup = await _context.SubMessageGroups
                .Where(w => w.MessageToUserId == userId)
                .AnyAsync(w => w.MessagesGroupId == messagesGroupsId);

            if (ifIAmIncludedInTheGroup)
            {
                groupMessageDetails = await (from messageGroup in _context.MessagesGroups

                                             join subMessage in _context.SubMessageGroups
                                             on messageGroup.Id equals subMessage.MessagesGroupId

                                             join userMessage in _context.SubUserMessages
                                             on subMessage.Id equals userMessage.SubMessageGroupId

                                             where groupBelongIds.Contains(subMessage.MessageToUserId)
                                             where messageGroup.Id == messagesGroupsId
                                             select new SpecificGroupMessages()
                                             {
                                                 ISent = subMessage.MessageToUserId == userId,
                                                 MessageCreation = userMessage.MessageCreation,
                                                 UserId = subMessage.MessageToUserId,
                                                 Username = subMessage.MessageToUser.FirstName + " " + subMessage.MessageToUser.LastName,
                                                 UserMessage = userMessage.Body,
                                                 UserProfile = subMessage.MessageToUser.ImageUrl,
                                                 SubUserMessageId = subMessage.Id
                                             }).OrderByDescending(w => w.MessageCreation)
                  .AsNoTracking().ToListAsync();
            }


            var myChatResult = await _context.SubMessageGroups
                          .Include(w => w.MessagesGroup)
                          .Where(w => w.MessageToUserId == userId)
                          .FirstOrDefaultAsync(w => w.MessagesGroupId == messagesGroupsId);

            if (myChatResult != null)
            {
                groupDetails = new MyGroupDetails()
                {
                    GroupName = myChatResult.MessagesGroup.GroupName,
                    SubMessageGroupId = myChatResult.Id
                };
            }

            return new MessageToGroupPostGetView()
            {
                TheGroupMessages = groupMessageDetails,
                MyGroupDetails = groupDetails
            };
        }

        public async Task<MessagesGroup> PostCreateGroupMessage(string userId, SubMessageGroupCreationDTO dto)
        {
            /* var otherIds = await _context.SubMessageGroups
               .Where(w => w.MessageToUserId != userId)
               .Distinct()
               .ToListAsync();

             var myId = await _context.SubMessageGroups
                 .Where(w => w.MessageToUserId == userId)
                 .Select(w => w.MessagesGroupId)
                 .Distinct()
                 .ToListAsync();

             var sameGroupId = otherIds.Where(w => myId.Contains(w.MessagesGroupId)).Distinct();

             var myGroupChatId = sameGroupId.Select(w => w.MessageToUserId).OrderBy(w => w).Distinct().ToList();

             if (dto.MessageToUserId.Contains(userId))
             {
                 return ;
             }
             // If the same user who had created Group with the SAME other users would should be avoided.
             if (myGroupChatId.SequenceEqual(dto.MessageToUserId.OrderBy(w => w))
                 && myGroupChatId.Count() == dto.MessageToUserId.Count())
             {
                 return  ;
             }

             *//*var con = otherIds.Concat(myId).ToList();*//*

             // Selecting a user must be two or more
             if (dto.MessageToUserId.Count > 1)
             {
                 var group = new MessagesGroup()
                 {
                     GroupCreation = DateTime.Now,
                     UserMadeById = userId,
                     SubMessageGroups = BuildMessageGroup(dto.MessageToUserId, userId),
                     GroupName = dto.GroupTitle
                 };

                 await _context.MessagesGroups.AddAsync(group);
                 await _context.SaveChangesAsync();
                 *//*return Ok($"Group '{group.GroupName}' has been created.");*//*

             }*/

            var group = new MessagesGroup()
            {
                GroupCreation = DateTime.Now,
                UserMadeById = userId,
                SubMessageGroups = BuildMessageGroup(dto.MessageToUserId, userId),
                GroupName = dto.GroupTitle
            };

            await _context.MessagesGroups.AddAsync(group);
            await _context.SaveChangesAsync();
            return group;

        }
        private List<SubMessageGroup> BuildMessageGroup(List<string> messageToUserId, string userId)
        {
            var r = new List<SubMessageGroup>();

            messageToUserId.Add(userId);

            if (messageToUserId != null)
            {
                foreach (var item in messageToUserId)
                {

                    r.Add(new SubMessageGroup()
                    {
                        MessageToUserId = item,
                    });
                }

            }
            return r;
        }

        public async Task<List<string>> CheckMyGroupChatId(string userId)
        {
            var otherIds = await _context.SubMessageGroups
               .Where(w => w.MessageToUserId != userId)
               .Distinct()
               .ToListAsync();

            var myId = await _context.SubMessageGroups
                .Where(w => w.MessageToUserId == userId)
                .Select(w => w.MessagesGroupId)
                .Distinct()
                .ToListAsync();

            var sameGroupId = otherIds.Where(w => myId.Contains(w.MessagesGroupId)).Distinct();

            var myGroupChatId = sameGroupId.Select(w => w.MessageToUserId).OrderBy(w => w).Distinct().ToList();
            return myGroupChatId;
/*            if (dto.MessageToUserId.Contains(userId))
            {
                return BadRequest("You dont need to include yourself in the group..");

            }*/
            // If the same user who had created Group with the SAME other users would should be avoided.

            ////
            /*            if (myGroupChatId.SequenceEqual(dto.MessageToUserId.OrderBy(w => w))
                            && myGroupChatId.Count() == dto.MessageToUserId.Count())
                        {
                            return BadRequest("You have already created a group with the same users.");
                        }*/
        }

        public async Task<bool> PostMessageToGroup(string userId, MessageToGroupCreation dto)
        {
            var checkAlignedUserToMessage = await _context.SubMessageGroups
                 .Where(w => w.MessageToUserId == userId)
                 .AnyAsync(w => w.Id == dto.SubMessageGroupId);

            // Check all bug
            // Refactor
            if (checkAlignedUserToMessage == true)
            {
                var group = new SubUserMessages()
                {
                    Body = dto.Body,
                    MessageCreation = DateTime.Now,
                    SubMessageGroupId = dto.SubMessageGroupId,
                };



                await _context.SubUserMessages.AddAsync(group);
                await _context.SaveChangesAsync();

            }
            return checkAlignedUserToMessage;
        }
    }
}
