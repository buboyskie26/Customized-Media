using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public string Image { get; set; }
        public AppUser Author { get; set; }
        public string AuthorId { get; set; }
        public AppUser ActivityUser { get; set; }
        public string ActivityUserId { get; set; }
        [ForeignKey("ActivityId")]
        public Activity Activity { get; set; }
        public int ActivityId { get; set; }
        public ICollection<CommentReaction> CommentReactions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
