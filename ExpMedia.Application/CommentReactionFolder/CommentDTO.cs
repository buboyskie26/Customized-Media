using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.CommentReactionFolder
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public string Image { get; set; }
        public string AuthorId { get; set; }
        public int LikeCount { get; set; }
        public int HeartCount { get; set; }

        public int ActivityId { get; set; }
        public List<Reaction> CommentReactions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class Reaction
    {
        public int Id { get; set; }
        public int Like { get; set; }
        public int Unlike { get; set; }
        public int Heart { get; set; }
        public int CommentId { get; set; }
        public string UserId { get; set; }
        public DateTime DateReact { get; set; }
    }
}
