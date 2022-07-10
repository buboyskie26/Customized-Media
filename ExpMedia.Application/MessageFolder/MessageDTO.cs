using ExpMedia.Application.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.MessageFolder
{
    public class MessageDTO
    {
        public int MessageId { get; set; }
        public int MessageTableId { get; set; }
        public string Body { get; set; }
        public string MessageFromId { get; set; }
        public string MessageFromUsername { get; set; }
        public DateTime DateMessage { get; set; }
    }
    public class InboxView
    {
        public ICollection<MessageUsers> MyFriends { get; set; }
        public ICollection<MyGroupChat> MyGroupChats { get; set; }
    }
    public class CommonUsers
    {
        public string UserId { get; set; }
        public string Username { get; set; }
    }
    public class MessageUsers : CommonUsers
    {
        public string ImageUrl { get; set; }

    }
    public class MessageSingleUsers: MessageUsers
    {
        public int MessageTableId { get; set; }
     
    }
    public class MyGroupChat
    {
        public string GroupName { get; set; }
        public string Username { get; set; }
        public int MessagesGroupId { get; set; }
        public int NumberOfUser { get; set; }
        public string LatestChat { get; set; }
    }
    public class FilterFollowingsDTO
    {
        public int Page { get; set; }
        public int RecordsPerPage { get; set; }
        public PaginationDTO PaginationDTO
        {
            get
            {
                return new PaginationDTO()
                {
                    Page = Page,
                    RecordsPerPage = RecordsPerPage
                };
            }
        }
        // Filter by username.
        public string Username { get; set; }
    }
    public class MessageToGroupPostGetView
    {

        public MyGroupDetails MyGroupDetails { get; set; }
/*        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserMessage { get; set; }
        public DateTime MessageCreation { get; set; }
        public bool ISent { get; set; }*/
        public ICollection<SpecificGroupMessages> TheGroupMessages { get; set; }

    }
    public class MyGroupDetails
    {
        public int SubMessageGroupId { get; set; }
        public int MessageGroupId { get; set; }
        public string GroupName { get; set; }
    }
    public class SpecificGroupMessages : CommonUsers
    {
        public int SubMessageGroupId { get; set; }
        public int SubUserMessageId { get; set; }
        public string UserProfile { get; set; }
        public string UserMessage { get; set; }
        public DateTime MessageCreation { get; set; }
        public bool ISent { get; set; }

    }
}
