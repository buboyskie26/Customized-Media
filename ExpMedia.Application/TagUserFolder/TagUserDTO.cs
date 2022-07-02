using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.TagUserFolder
{
    public class TagUserDTO : ActivityCommon
    {
    
        public ICollection<TagInfo> TagInfos { get; set; }
    }
    public class TagInfo
    {
        public int TagId { get; set; }
        public DateTime TagCreated { get; set; }
        public bool DidYouCreatedTheTagged { get; set; }
        public string TagUsername { get; set; }
    }
    public class ActivityCommon
    {
        public int ActivityId { get; set; }
        public string Title { get; set; }
        public DateTime DateOfActivity { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
    }
}
