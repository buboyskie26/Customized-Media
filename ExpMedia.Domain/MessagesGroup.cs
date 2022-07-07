using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class MessagesGroup
    {
        public int Id { get; set; }
        public List<SubMessageGroup> SubMessageGroups { get; set; }


        [ForeignKey("UserMadeById")]
        public AppUser UserMadeBy { get; set; }
        public string UserMadeById { get; set; }
        public DateTime GroupCreation { get; set; }
        public string GroupName { get; set; }

    }
}
