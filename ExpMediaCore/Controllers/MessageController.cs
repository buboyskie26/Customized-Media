using ExpMedia.Domain;
using ExpMedia.Persistence;
using ExpMediaCore.Repository.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IActivity _activity;

        public MessageController(DataContext context, UserManager<AppUser> userManager,
            IActivity activity)
        {
            _context = context;
            _userManager = userManager;
            _activity = activity;
        }

        [HttpPost("messageTo/{userId}")]
        public async Task<ActionResult> PostActivity(string userId)
        {
            var user = await _userManager.GetUserAsync(User);
             
            return NoContent();

        }


    }
}
