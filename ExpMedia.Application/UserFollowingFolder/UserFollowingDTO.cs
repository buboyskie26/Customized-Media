using ExpMedia.Application.ActivitiyFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.UserFollowingFolder
{
    public class UserFollowingDTO
    {
    }
    public class ProfileView
    {
        public ICollection<UserProfile> UserProfiles { get; set; }
        public ICollection<ActivityView> MyPosts { get; set; }
        public ICollection<ActivityView> FollowedUserPosts { get; set; }
    }
    public class UserProfile
    {
        public string UserId { get; set; }
        public bool IsFollower { get; set; }
        public string FullName { get; set; }
        public string ImageUrl { get; set; }
        public string Bio { get; set; }
        public bool IsOwned { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public ICollection<UserInformation> FollowingsUserInfo { get; set; }
        public ICollection<UserInformation> FollowersUserInfo { get; set; }
    }
    public class UserInformation 
    {
        public string FullName { get; set; }
        public string UserId { get; set; }


    }
    public class FollowingNotifView: UserInformation
    {
        public DateTime DateFollowedYou { get; set; }
        public int FollowId { get; set; }

    }
}
