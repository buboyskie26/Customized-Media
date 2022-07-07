using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class SubUserMessages
    {
        public int Id { get; set; }
        public DateTime MessageCreation { get; set; }
        [MaxLength(80)]
        public string Body { get; set; }
        [ForeignKey("SubMessageGroupId")]
        // Changed the name
        public SubMessageGroup SubMessageGroup { get; set; }
        public int SubMessageGroupId { get; set; }
        public int GroupId { get; set; }
    }
}
