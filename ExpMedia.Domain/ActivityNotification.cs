using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class ActivityNotification
    {
        public int Id { get; set; }
        public Activity Activity { get; set; }
        public int ActivityId { get; set; }
        public DateTime NotificationTime { get; set; }
        public AppUser NotifyTo { get; set; }
        public string NotifyToId { get; set; }
        public AppUser NotifyFrom { get; set; }
        public string NotifyFromId { get; set; }
        public bool IsSeen { get; set; }

    }
}
