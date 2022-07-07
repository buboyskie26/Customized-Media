using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class SubMessageGroup
    {
        public int Id { get; set; }
        public List<SubUserMessages> SubUserMessagesx { get; set; }

        [ForeignKey("MessageToUserId")]
        public AppUser MessageToUser { get; set; }
        public string MessageToUserId { get; set; }
     
        [ForeignKey("MessagesGroupId")]
        public MessagesGroup MessagesGroup { get; set; }
        public int MessagesGroupId { get; set; }
    }
}
