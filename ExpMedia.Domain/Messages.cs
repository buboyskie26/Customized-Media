using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class Messages
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Body { get; set; }
/*        [ForeignKey("MessageToUserId")]
        public AppUser MessageToUser { get; set; }
        public string MessageToUserId { get; set; }*/

        [ForeignKey("MessageById")]
        public AppUser MessageBy{ get; set; }
        public string MessageById { get; set; }
        public DateTime MessageCreated { get; set; }

        [ForeignKey("MessageTableId")]
        public MessageTable MessageTable { get; set; }
        public int MessageTableId { get; set; }

    }
}
