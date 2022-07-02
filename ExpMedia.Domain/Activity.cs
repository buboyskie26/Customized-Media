using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class Activity 
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ActivityUserId { get; set; }
        public DateTime DateOfActivity { get; set; }
        public DateTime DateCreated { get; set; }
        public AppUser ActivityUser { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public bool IsCancelled { get; set; }
        public bool OnlyMe { get; set; }
        public bool IsSelectedPost { get; set; }
        public ICollection<ActivityAttendee> Attendees { get; set; }
        public ICollection<SharingActivity> SharingActivities { get; set; }
        public ICollection<ActivityNotification> ActivityNotifications { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<ActivityUserSelection> ActivityUserSelections { get; set; }
        public ICollection<TagUser> TagUsers { get; set; }

    }
}
