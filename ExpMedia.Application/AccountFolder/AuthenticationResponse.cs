using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.AccountFolder
{
    public class AuthenticationResponse
    {
        public string Token { get; set; }
        public string RoleName { get; set; }
        public DateTime Expiration { get; set; }
    }
}
