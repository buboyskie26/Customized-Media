using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.MessageGroupFolder
{
    public class SubMessageGroupCreationDTO
    {
        public List<string> MessageToUserId { get; set; }
        public  string  GroupTitle { get; set; }
    }
    public class MessageToGroupCreation
    {
 
        public string Body { get; set; }
        public int SubMessageGroupId { get; set; }
    }
}
