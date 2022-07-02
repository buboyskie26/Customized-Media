using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class ActivityUserSelection
    {
        public int Id { get; set; }
        public Activity Activity { get; set; }
        public int ActivityId { get; set; }

        public AppUser Users { get; set; }
        public string UsersId { get; set; }

        public AppUser CreatedUser { get; set; }
        public string CreatedUserId { get; set; }
    }
}
