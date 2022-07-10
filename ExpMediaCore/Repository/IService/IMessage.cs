using ExpMedia.Application.MessageFolder;
using ExpMedia.Application.MessageGroupFolder;
using ExpMedia.Domain;
using ExpMediaCore.BaseRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.Repository.IService
{
    public interface IMessage  
    {
        string GetUserId();
        Task<bool> PostMessageActivity(string userId, MessageCreationDTO dto);
        Task<Tuple<bool, bool>> PostCreateMessageTo(string userId, MessageTableCreationDTO dto);
        Task<List<MessageDTO>> GetMessageFrom(string userId, int messageTableId);
        Task<List<MessageSingleUsers>> GetMessageView(string userId );
        Task<InboxView> GetUserIHadFollowed(string userId, FilterFollowingsDTO dto);
        Task<List<MessageDTO>> GetMessageFromUserId(string userId, string chatUserId);
        Task<MessageToGroupPostGetView> GetMessageToGroupPostGet(string userId, int messagesGroupsId);
        Task<MessagesGroup> PostCreateGroupMessage(string userId, SubMessageGroupCreationDTO dto);
        Task<List<string>> CheckMyGroupChatId(string userId);
        Task<bool> PostMessageToGroup(string userId, MessageToGroupCreation dto);
        Task<Tuple<string, bool>> DeleteMessage(string userId, int subUserMessageId);
        Task<bool> LeeavingTheGroup(string userId, int subMessageGroupId);
        Task<bool> RangedDeleteOtherUserFromTheGroup(string userId, GroupMemberToDeleteRangeDTO dto);

    }
}
