using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class ActivityAttendee
    {
        public int Id { get; set; }
        public bool? IsAccepted { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }
        public DateTime DateJoined { get; set; }
        public DateTime DateRequest { get; set; }
        public string ActivityCreatedUserId { get; set; }

    }
}
