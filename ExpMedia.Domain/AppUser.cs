using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpMedia.Domain
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageUrl { get; set; }
        public string Role { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string Bio { get; set; }
        public ICollection<ActivityAttendee> Activitie { get; set; }
        public ICollection<Activity> ActivityUsers { get; set; }
        public ICollection<SharingActivity> SharingActivitiesUsers { get; set; }
        public ICollection<UserFollowing> Followings { get; set; }
        public ICollection<Comment> AuthorUsers { get; set; }
        public ICollection<TagUser> UsersTagged { get; set; }
        public ICollection<BlockUsers> ListOfToBlockUser { get; set; }
        public ICollection<ActivityNotification> NotifyToUser { get; set; }
        public ICollection<CommentReaction> CommentUser { get; set; }
        /*        public ICollection<UserFollowing> Followers { get; set; }*/

        /*   public ICollection<Photo> Photos { get; set; }
           public ICollection<UserFollowing> Followings { get; set; }
           public ICollection<UserFollowing> Followers { get; set; }
           public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();*/
    }
}
