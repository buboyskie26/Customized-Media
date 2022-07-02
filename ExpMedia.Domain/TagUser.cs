using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class TagUser
    {
        public int Id { get; set; }
        public virtual Activity Activity { get; set; }
        public int ActivityId { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual AppUser UserToTag { get; set; }
        public string UserToTagId { get; set; }

        public virtual AppUser UserWhoTagged { get; set; }
        public string UserWhoTaggedId { get; set; }
    }
}
