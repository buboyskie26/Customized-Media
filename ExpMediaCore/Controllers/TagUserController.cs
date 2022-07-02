using AutoMapper;
using ExpMedia.Application.TagUserFolder;
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
    public class TagUserController : ControllerBase
    {
        private readonly IMapper _map;
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;

        public TagUserController(IMapper map, DataContext context, UserManager<AppUser> userManager)
        {
            _map = map;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("tagFeed")]
        public async Task<ActionResult<List<TagUserDTO>>> TagFeed()
        {
            var user = await _userManager.GetUserAsync(User);
            var result = new List<TagUserDTO>();


            var o = await _context.TagUsers
            .Include(w => w.Activity)
            .Include(w => w.UserToTag)
            .ToListAsync();


            var allUsers = await _context.AppUser
            .Where(w => w.Id == user.Id)
            .Select(w => w.Id)
            .ToListAsync();

            var tagUserIds = _context.TagUsers.Select(w => w.UserToTagId).Distinct().ToList();
            var checkITaggedSomeone = _context.TagUsers
                .Where(w => w.UserWhoTaggedId == user.Id)
                .Select(w => w.UserWhoTaggedId)
                .Distinct().ToList();

            var combinedIds = tagUserIds.Concat(checkITaggedSomeone);

            var i = o.Where(w => allUsers.Contains(w.UserToTagId));

            // If the logged in user is belong to the tagged post, it will show to his/her tagUser feed.

            // Next step: Add another tagUser column in db to show if the results has a different value.
            var item = await (from t in _context.TagUsers
                              join ty in _context.Activities on t.ActivityId equals ty.Id

                              where allUsers.Contains(t.UserWhoTaggedId) || allUsers.Contains(t.UserToTagId)
                              orderby t.DateCreated
                              select new TagUserDTO
                              {
                                  Category = ty.Category,
                                  City = ty.City,
                                  DateOfActivity = ty.DateOfActivity,
                                  Title = ty.Title,
                                  Description = ty.Description,
                                  Venue = ty.Venue,
                                  ActivityId = ty.Id,
                                  TagInfos = (from p in ty.TagUsers
                                              select new TagInfo()
                                              {
                                                  DidYouCreatedTheTagged = p.UserWhoTaggedId == user.Id,
                                                  TagCreated = p.DateCreated,
                                                  TagId = p.Id,
                                                  TagUsername = p.UserToTag.FirstName + " " + p.UserToTag.LastName,

                                              }).ToList()
                              }).AsNoTracking().ToListAsync();
            item = item.GroupBy(w => w.ActivityId).Select(w => w.First()).ToList();
            return item;
        }

        [HttpPost("taggingSomeone")]
        public async Task<ActionResult> TagUser([FromForm] TagUserCreationDTO d)
        {
            var user = await _userManager.GetUserAsync(User);

            var created = new Activity()
            {
                Venue = d.Venue,
                ActivityUserId = user.Id,
                City = d.City,
                DateCreated = DateTime.Now,
                IsCancelled = false,
                Description = d.Description,
                Title = d.Title,
                Category = d.Category,
                DateOfActivity = DateTime.Now.AddDays(3),
                OnlyMe = false,
                IsSelectedPost = true
            };
            await _context.Activities.AddAsync(created);
            await _context.SaveChangesAsync();

            var p = (from t in d.UserIds
                     select new TagUser()
                     {
                         ActivityId = created.Id,
                         UserToTagId = t,
                         UserWhoTaggedId = user.Id
                     }).ToList();

            await _context.TagUsers.AddRangeAsync(p);
            await _context.SaveChangesAsync();

            return Ok("Successfully created an activity and tagged someone");
        }
        // Needs to have a PUTGET.
        [HttpPut("modifyingTag")]
        public async Task<ActionResult> AddingExistingTaggedUser([FromForm] TagUserAddExistingDTO d)
        {
            var user = await _userManager.GetUserAsync(User);

            var tagPost = await _context.TagUsers
                .Where(w => w.UserWhoTaggedId == user.Id)
                .FirstOrDefaultAsync(w => w.ActivityId == d.ActivityId);

            var tagExists = await _context.TagUsers
                .AnyAsync(w => w.ActivityId == d.ActivityId);

            if (tagExists)
            {
                // All the remaining record in the db will br stayed in.
                // All users you had selected will be added togethwe with the remaining record in the db.
                var p = (from t in d.UserIds
                         select new TagUser()
                         {
                             ActivityId = tagPost.ActivityId,
                             UserToTagId = t,
                             UserWhoTaggedId = user.Id,
                             DateCreated = DateTime.Now
                         }).ToList();

                _context.TagUsers.UpdateRange(p);
                await _context.SaveChangesAsync();

                return Ok("Successfully added some users in the existing activity.");

            }
            else
            {
                return BadRequest("Activity Post doesnt exists.");

            }
            return NoContent();

        }
        [HttpDelete("deleteTagged")]
        public async Task<ActionResult> RemovingTaggedUser([FromForm] TagUserRemovingDTO d)
        {
            var user = await _userManager.GetUserAsync(User);

            var tagPost = await _context.TagUsers
                .Where(w => w.UserWhoTaggedId == user.Id)
                .FirstOrDefaultAsync(w => w.ActivityId == d.ActivityId);

            var tagExists = await _context.TagUsers
                .AnyAsync(w => w.ActivityId == d.ActivityId);

            var tagIds = _context.TagUsers
                .Select(w => w.UserToTagId).ToList();

            var asd = _context.TagUsers
            .Where(w => d.UserIds.Contains(w.UserToTagId) == true)
            .ToList();

            var idToRemove = tagIds.Where(w => d.UserIds.Contains(w)).ToList();

            if (tagExists)
            {
 
                _context.TagUsers.RemoveRange(asd);
                await _context.SaveChangesAsync();

                return Ok("Successfully removing some users in the existing activity.");

            }
            return NoContent();

        }
    }
}
