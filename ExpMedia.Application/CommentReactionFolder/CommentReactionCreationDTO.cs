using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.CommentReactionFolder
{
    public class CommentReactionCreationDTO
    {
        public int CommentId { get; set; }
        public int ActivityId { get; set; }
        public string CommentCreatedUserId { get; set; }
    }
}
