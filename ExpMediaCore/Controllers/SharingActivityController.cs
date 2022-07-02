using AutoMapper;
using ExpMedia.Application.SharingActivityFolder;
using ExpMedia.Domain;
using ExpMedia.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharingActivityController : ControllerBase
    {
        private readonly IMapper _map;
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SharingActivityController(IMapper map, DataContext context, UserManager<AppUser> userManager)
        {
            _map = map;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("sharingUserPost")]
        public async Task<ActionResult> SharingPostActivity([FromBody] SharingActivityCreationDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);

            var activityUser = await _context.Activities
                .Include(w => w.ActivityUser)
                .FirstOrDefaultAsync(w => w.Id == dto.ActivityId);

            // Get own userFollowing DB.
            var follow = await _context.UserFollowings
                .Include(w=> w.UserWhoFollowed)
                .Include(w=> w.UserToFollow)
                .Where(w=> w.UserWhoFollowedId == user.Id)
                .ToListAsync();

            var ifIFollowedActivityUser = follow.Where(w => activityUser.ActivityUserId == w.UserToFollowId).Any();

            var onlyOnce = await _context.SharingActivities
                .Where(w=> w.SharedUserId == user.Id)
                .Where(w=> activityUser.Id == w.ActivityId)
                .ToListAsync();

            


            if (ifIFollowedActivityUser)
            {
                if(onlyOnce.Count() <= 0)
                {
                    var obj = new SharingActivity()
                    {
                        ActivityId = dto.ActivityId,
                        SharedUserId = user.Id
                    };
/*
                    await _context.SharingActivities.AddAsync(obj);
                    await _context.SaveChangesAsync();*/
                    return Ok("Succesfully shared a post.");
                }
                else
                {
                    return BadRequest("Only once you could shared post from the same user..");

                }


            }
 


            return BadRequest("You must follow that user before you shared his/her post.");
        }

    }
}
