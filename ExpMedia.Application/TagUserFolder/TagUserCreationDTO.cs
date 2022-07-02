using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.TagUserFolder
{
    public class TagUserCreationDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public List<string> UserIds { get; set; }

        /*        public int ActivityId { get; set; }
                public string UserToTagId { get; set; }*/
    }

    public class TagUserAddExistingDTO
    {
        public int ActivityId { get; set; }
        public List<string> UserIds { get; set; }
    }
    public class TagUserRemovingDTO
    {
        public int ActivityId { get; set; }
        public List<string> UserIds { get; set; }
    }
}
