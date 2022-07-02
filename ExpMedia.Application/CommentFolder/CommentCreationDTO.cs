using ExpMedia.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.CommentFolder
{
    public class CommentCreationDTO
    {
        public int ActivityId { get; set; }
        public string ActivityUserId { get; set; }
        [MaxLength(100)]
        public string Body { get; set; }
        public IFormFile Image { get; set; }
    }
}
