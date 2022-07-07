using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.MessageFolder
{
    public class MessageCreationDTO
    {
        public int MessageTableId { get; set; }

/*        public string MessageToId { get; set; }*/
        public string Body { get; set; }
    }
    public class MessageTableCreationDTO
    {
        public string MessageToId { get; set; }
        public string Body { get; set; }
    }
  
}
