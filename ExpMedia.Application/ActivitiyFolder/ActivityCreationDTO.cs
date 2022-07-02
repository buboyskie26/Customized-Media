using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.ActivitiyFolder
{
    public class ActivityCreationDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
    }
    public class ActivityUserCreationDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public List<string> UserIds { get; set; }
     
    }
}
