using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class SharingActivity
    {
        public int Id { get; set; }
        public Activity Activity { get; set; }
        public int ActivityId { get; set; }
        public DateTime DateShared { get; set; } = DateTime.UtcNow;

        public AppUser SharedUser { get; set; }
        public string SharedUserId { get; set; }
        public bool OnlyMe { get; set; }

    }
}
