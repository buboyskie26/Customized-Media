using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class UserFollowing
    {
        public int Id { get; set; }
        // User who had been followed by other user
        public string UserWhoFollowedId { get; set; }
        [ForeignKey("UserWhoFollowedId")]
        public virtual AppUser UserWhoFollowed { get; set; }
        // User who wants to follow another user
        public string UserToFollowId { get; set; }
        [ForeignKey("UserToFollowId")]
        public virtual AppUser UserToFollow { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public bool IsSeen { get; set; }

    }
}
