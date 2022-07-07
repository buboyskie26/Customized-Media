using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class MessageTable
    {
        public int Id { get; set; }
        public List<Messages> Messagesx { get; set; }

        [ForeignKey("MessageToUserId")]
        public AppUser MessageToUser { get; set; }
        public string MessageToUserId { get; set; }
        [ForeignKey("MessageById")]
        public AppUser MessageBy { get; set; }
        public string MessageById { get; set; }
    }
}
