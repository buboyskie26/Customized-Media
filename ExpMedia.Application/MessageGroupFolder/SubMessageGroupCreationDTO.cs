using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.MessageGroupFolder
{
    public class SubMessageGroupCommon
    {
        public int SubMessageGroupId { get; set; }

    }
    public class SubMessageGroupCreationDTO
    {
        public List<string> MessageToUserId { get; set; }
        public  string  GroupTitle { get; set; }
    }
    public class MessageToGroupCreation : SubMessageGroupCommon
    {
 
        public string Body { get; set; }
    }

    public class GroupMemberToDeleteDTO : SubMessageGroupCommon
    {

        public int MessageGroupId { get; set; }
    }
    public class GroupMemberToDeleteRangeDTO  
    {
        public List<int> SubMessageGroupId { get; set; }

        public int MessageGroupId { get; set; }
    }
}
