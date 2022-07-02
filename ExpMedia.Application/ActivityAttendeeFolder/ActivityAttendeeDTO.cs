using ExpMedia.Application.ActivitiyFolder;
using ExpMedia.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.ActivityAttendeeFolder
{
    public class ActivityAttendeeDTO
    {
        public int Id { get; set; }
        public string ActivityAttendeeUserId { get; set; }
        public AppUser ActivityAttendeeUser { get; set; }
        public int ActivityId { get; set; }
        public ActivityDTO Activity { get; set; }
        public DateTime DateJoined { get; set; }
    }
    public class AcceptedActivityRoom
    {
        public int ActivityId { get; set; }
        public string ActivityTitle { get; set; }
        public DateTime DateOfActivity { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public bool IsCancelled { get; set; }
        public ICollection<OtherUsers> OtherAcceptedUsers { get; set; }

    }
    public class OtherUsers
    {
        public string Username { get; set; }

    }
    public class InsideActivityRoomView
    {
        public int ActivityId { get; set; }
        public int CommentId { get; set; }
        public string ActivityTitle { get; set; }
        public DateTime DateOfActivity { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public bool IsJoined { get; set; }
        public ICollection<UserComment> UserComments { get; set; }

    }
       
    public class UserComment
    {
        public string Username { get; set; }
        public string Image { get; set; }
        public string Body { get; set; }
        public string UserId{ get; set; }
        public DateTime CommentCreated{ get; set; }

    }


}

