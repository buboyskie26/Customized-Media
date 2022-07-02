using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.ActivitiyFolder
{
    public  class ActivityDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DateOfActivity { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public bool IsCancelled { get; set; }

        public ICollection<AttendeesRequest> AttendeesRequests { get; set; }
        public ICollection<AttendeeMember> AttendeeMembers { get; set; }
        public ICollection<ActivityUserComment> UserComments { get; set; }

    }
    public class AttendeeMember
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public DateTime DateJoined { get; set; }
        public int ActivityAttendeeId { get; set; }

    }
    public class AttendeesRequest
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public DateTime RequestJoin { get; set; }
        public int ActivityAttendeeId { get; set; }
        public bool? IsAccepted { get; set; }

    }
    public class ActivityCommon
    {
        public int ActivityId { get; set; }
        public string Title { get; set; }
        public DateTime DateOfActivity { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
    }

    public class ActivityView : ActivityCommon
    {
        public int Id { get; set; }
        public int Shares { get; set; }
        public DateTime DateCreation { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsOnlyMe { get; set; }
        public bool IsPostSelected { get; set; }
        public string ActivityUserName { get; set; }
        public ICollection<Attendee> Attendees { get; set; }
        public ICollection<ShareActivityView> SharePosts { get; set; }
        
    }
    public class ActivityAndNotifView
    {
        public ICollection<ActivityView> ActivityFeed { get; set; }
        public ICollection<ActivityNotificationView> ActivityNotifications { get; set; }

    }

    public class ActivityNotificationView
    {
        public int ActivityId { get; set; }
        public string RequestFromId { get; set; }
        public string RequestFromName { get; set; }

    }
    public class Attendee 
    {
        public int Id { get; set; }
        public bool? IsAccepted { get; set; }
        public string UserId { get; set; }
        public int ActivityId { get; set; }
        public DateTime DateJoined { get; set; }


    
    }
    public class ShareActivityView : ActivityCommon
    {
        public string UserSharedPostId { get; set; }
        public string UserSharedPostName { get; set; }
        public bool IsOnlyMe { get; set; }
        public DateTime DateShared { get; set; }
    }
    public class ActivityUserComment
    {
        public int CommentId { get; set; }
        public string Username { get; set; }
        public int ActivityId { get; set; }
        public string Image { get; set; }
        public string Body { get; set; }
        public string UserId { get; set; }
        public DateTime CommentCreated { get; set; }
        public bool IsCommentOwned { get; set; }
        public int LikeCount { get; set; }
        public int HeartCount { get; set; }
        //
        /*        public int CommentReactionId { get; set; }
                public int Like { get; set; }
                public int Heart { get; set; }
                public string ReactUserName { get; set; }*/
        public ICollection<UserReactionToComment> UserReactionToComments { get; set; }
    }
    public class UserReactionToComment
    {
        public int CommentReactionId { get; set; }
        public int CommentId { get; set; }
        public int Like { get; set; }
        public int Heart { get; set; }
        public string Username { get; set; }
    }

    public class SearchUserView
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public string Bio { get; set; }
        public ICollection<UserActivities> ActivityOfUser { get; set; }
        public ICollection<UserSharedPosts> SharedPostOfUser { get; set; }

    }

    public class UserActivities
    {
        public int ActivityId { get; set; }
        public string Title { get; set; }
        public DateTime DateOfActivity { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }

    }
    public class UserSharedPosts
    {
        public int SharedPostId { get; set; }
        public DateTime DateShared { get; set; } = DateTime.UtcNow;
        public bool OnlyMe { get; set; }

        public ICollection<Sampp> SharedPostActivity { get; set; }

    }

    public class Sampp
    {
        public int ActivityId { get; set; }
        public string Title { get; set; }
        public DateTime DateOfActivity { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }

    }
    public interface UserInTheTagsView
    {
        public List<TaggedUsers> UserInTheTags { get; set; }

    }
    public class ActivityMemories : ActivityCommon, UserInTheTagsView
    {
        public string Time { get; set; }
        public List<TaggedUsers> UserInTheTags { get; set; }

        /*        public ICollection<ActivityCommon> ActivityPost { get; set; }*/

    }
    public class TaggedUsers
    {
        public string Username { get; set; }
    }
    public class MemoryView
    {
        public ICollection<ActivityMemories> MemoryActivities { get; set; }
        public ICollection<OwnActivity> MyPastActivities { get; set; }
    }
    public class OwnActivity : ActivityCommon
    {
        public string Time { get; set; }
       
    }

    public class SampView
    {
         
        public ICollection<CommentView> CommentsView { get; set; }
        public ICollection<AttendeesRequest> ActivityUserRequests { get; set; }
        public ICollection<CommentReactionUser> CommentReactors { get; set; }
        public ICollection<ShareActivityViewNotif> SharedActivities { get; set; }
        public ICollection<TagUserViewNotif> TaggedUsers { get; set; }

    }
    public class TagUserViewNotif
    {
        public int ActivityId { get; set; }
        public DateTime DateTagged { get; set; }
        public string WhoTaggedYou { get; set; }
    }
    public class ShareActivityViewNotif
    {
        public int ActivityId { get; set; }
        public DateTime DateShared { get; set; }
        public string WhoSharedYourActivity { get; set; }
        public bool IsSharedOnlyMe { get; set; }
    }
    public class CommentView
    {
        public int ActivityId { get; set; }
        public int CommentId { get; set; }
        public string Body { get; set; }
        public string Commentor { get; set; }
        public string Image { get; set; }
        public string AuthorId { get; set; }
    }
    public class CommentReactionUser
    {
        public int ActivityId { get; set; }
        public int CommentReactorId { get; set; }
        public string CommentReactor { get; set; }
        public int Likes { get; set; }
        public int Heart { get; set; }
    }
}
