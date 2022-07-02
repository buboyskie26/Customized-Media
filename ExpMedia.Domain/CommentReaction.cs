using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class CommentReaction
    {
        public int Id { get; set; }
        public int Like { get; set; }
        public int Unlike { get; set; }
        public int Heart { get; set; }
        [ForeignKey("CommentId")]
        public Comment Comment { get; set; }
        public int CommentId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
        public string UserId { get; set; }

        [ForeignKey("CommentCreatedUserId")]
        public AppUser CommentCreatedUser { get; set; }
        public string CommentCreatedUserId { get; set; }
        public DateTime DateReact { get; set; }


    }
}
