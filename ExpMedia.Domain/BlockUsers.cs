using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Domain
{
    public class BlockUsers
    {
        public int Id { get; set; }
        public string UserToBlockId { get; set; }
        public AppUser UserToBlock { get; set; }

        public string UserWhoBlockId { get; set; }
        public AppUser UserWhoBlock { get; set; }
        public DateTime BlockCreation { get; set; } = DateTime.UtcNow;

    }
}
