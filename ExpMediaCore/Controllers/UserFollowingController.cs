using AutoMapper;
using ExpMedia.Application.ActivitiyFolder;
using ExpMedia.Application.UserFollowingFolder;
using ExpMedia.Domain;
using ExpMedia.Persistence;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class UserFollowingController : ControllerBase
    {
        private readonly IMapper _map;
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserFollowingController(IMapper map, DataContext context, UserManager<AppUser> userManager)
        {
            _map = map;
            _context = context;
            _userManager = userManager;
        }
        [HttpPost("followerUser")]
        public async Task<ActionResult> FollowUser([FromBody] UserFollowingCreationDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);

            var countToUserFollowId = await _context.UserFollowings
                .Where(w => w.UserWhoFollowedId == user.Id)
                .Where(w => w.UserToFollowId == dto.UserToFollowId)
                .ToListAsync();

            // Get the users who you had blocked
            var block = await _context.BlockUsersx
            .Where(e => e.UserWhoBlockId == user.Id)
            .Where(w => w.UserToBlockId.Equals(dto.UserToFollowId))
            .ToListAsync();

            var ids = block.Select(w => w.UserToBlockId).ToList();

            var contains = ids.Where(w => dto.UserToFollowId == w).Any();

            var checkIfExists = await _context.AppUser
                .AnyAsync(w => w.Id == dto.UserToFollowId);

            if (checkIfExists)
            {
                if (countToUserFollowId.Count() <= 0)
                {
                    if (contains == false)
                    {
                        var obj = new UserFollowing()
                        {
                            UserToFollowId = dto.UserToFollowId,
                            UserWhoFollowedId = user.Id
                        };
                        await _context.UserFollowings.AddAsync(obj);
                        await _context.SaveChangesAsync();
                        return Ok("Successfully followed the user");
                    }
                    else
                    {
                        return BadRequest("That user is block from your account.");
                    }

                }
                else
                {
                    return BadRequest("Only once could follow user");
                }
            }
            else
            {
                return BadRequest("That id doesnt exists.");
            }



            return NoContent();

        }
        [HttpPut("removeFollowUser")]
        public async Task<ActionResult> RemoveFollowing([FromBody] UserFollowingCreationDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);

            var checkIfExists = await _context.AppUser
                .AnyAsync(w => w.Id == dto.UserToFollowId);
            if (checkIfExists)
            {
                var o = await _context.UserFollowings.FirstOrDefaultAsync(w => w.UserToFollowId == dto.UserToFollowId);
                _context.UserFollowings.Remove(o);
                await _context.SaveChangesAsync();

                return Ok("Unfollowed the user success");
            }

            return BadRequest("That user doesnt exists.");
        }
        [HttpGet("userProfile/{userId}/{followId:int}")]
        public async Task<ActionResult<ProfileView>> ProfileUser(string userId, int followId)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = new ProfileView();

            var magic = _context.AppUser
                .Include(w => w.Followings).ThenInclude(w => w.UserToFollow)
                .ToList();

            var followObj = await _context.UserFollowings
                .Where(w=> w.UserToFollowId == user.Id)
               .FirstOrDefaultAsync(w => w.Id == followId);
            if(followObj != null)
            {
                followObj.IsSeen = true;
                _context.UserFollowings.Update(followObj);
                await _context.SaveChangesAsync();
            }
            var res = await (from p in _context.AppUser
                         where p.Id == userId
                         select new UserProfile
                         {
                             UserId = userId,
                             Bio = p.Bio,
                             FullName = p.FirstName + " " + p.LastName,
                             ImageUrl = p.ImageUrl,
                             IsFollower = p.Followings.Any(w => w.UserWhoFollowedId == user.Id),
                             FollowersCount = p.Followings.Count(),
                             /* FollowingCount = p.Followers.Count(),*/
                             IsOwned = user.Id == userId,
                             /*                             FollowingsUserInfo = (from y in p.Followers
                                                                                select new UserInformation
                                                                                {
                                                                                    FullName = y.UserToFollow.FirstName + " " + y.UserToFollow.LastName,
                                                                                    UserId = y.UserToFollowId,
                                                                                }).ToList(),*/

                             FollowersUserInfo = (from y in p.Followings
                                                  select new UserInformation
                                                  {
                                                      FullName = y.UserWhoFollowed.FirstName + " " + y.UserWhoFollowed.LastName,
                                                      UserId = y.UserWhoFollowedId,

                                                  }).ToList(),
                         }).AsNoTracking().ToListAsync();

            var myPost = await (from a in _context.Activities
                                where a.ActivityUserId == user.Id
                                select new ActivityView
                                {
                                    Id = a.Id,
                                    DateOfActivity = a.DateOfActivity,
                                    Category = a.Category,
                                    City = a.City,
                                    Venue = a.Venue,
                                    IsCancelled = a.IsCancelled,
                                    Title = a.Title,
                                    Description = a.Description,
                                    ActivityUserName = a.ActivityUser.FirstName + " " + a.ActivityUser.LastName,
                                    DateCreation = a.DateCreated,
                                    IsOnlyMe = a.OnlyMe == true

                                }).AsNoTracking().OrderBy(e => e.DateCreation).ToListAsync();

            var userPostIHadFollowed = await (from a in _context.Activities
                                              where a.ActivityUserId == userId
                                              where a.OnlyMe == false

                                              select new ActivityView
                                              {
                                                  Id = a.Id,
                                                  DateOfActivity = a.DateOfActivity,
                                                  Category = a.Category,
                                                  City = a.City,
                                                  Venue = a.Venue,
                                                  IsCancelled = a.IsCancelled,
                                                  Title = a.Title,
                                                  Description = a.Description,
                                                  ActivityUserName = a.ActivityUser.FirstName + " " + a.ActivityUser.LastName,
                                                  DateCreation = a.DateCreated,
                                                  IsOnlyMe = a.OnlyMe == true
                                              }).AsNoTracking().OrderBy(e => e.DateCreation).ToListAsync();

            var asd = userPostIHadFollowed.Any(w => w.IsOnlyMe == false);

            return new ProfileView()
            {
                UserProfiles = res,
                /*                FollowedUserPosts = userPostIHadFollowed.Any(w => w.IsOnlyMe == false) && res.Any(w => w.IsFollower == true) ? userPostIHadFollowed : null,*/
                FollowedUserPosts = res.Any(w => w.IsFollower == true) ? userPostIHadFollowed : null,
                MyPosts = res.Any(w => w.IsOwned == true) ? myPost : null,
                /*      FollowedUserPosts = res.Any(w => w.UserId == user.Id) ? null : userPostIHadFollowed,*/
            };

        }


        [HttpGet("followingNotification")]
        public async Task<ActionResult<List<FollowingNotifView>>> FollowingNotification()
        {
            var user = await _userManager.GetUserAsync(User);

            var item = await (from p in _context.UserFollowings
                              where p.UserToFollowId == user.Id
                              select new FollowingNotifView
                              {
                                  DateFollowedYou = p.DateCreation,
                                  FullName=p.UserWhoFollowed.FirstName+" "+p.UserWhoFollowed.LastName,
                                  UserId = p.UserWhoFollowedId,
                                  FollowId = p.Id,
                              }).ToListAsync();
            return item;
        }
    }
}