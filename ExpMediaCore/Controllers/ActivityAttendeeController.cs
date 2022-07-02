using AutoMapper;
using ExpMedia.Application.ActivityAttendeeFolder;
using ExpMedia.Domain;
using ExpMedia.Persistence;
using ExpMediaCore.Repository.IService;
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
    public class ActivityAttendeeController : ControllerBase
    {
        private readonly IMapper _map;
        private readonly IActivityAttendeeRepository _activityAttendee;
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ActivityAttendeeController(DataContext context, IMapper map,
            UserManager<AppUser> userManager, IActivityAttendeeRepository activityAttendee)
        {
            _context = context;
            _map = map;
            _userManager = userManager;
            _activityAttendee = activityAttendee;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActivityAttendee>>> GetAllActivities()
        {
            return await _context.ActivityAttendees
                .AsNoTracking().ToListAsync();
        }

        [HttpPost("requestToJoined/{activityId}")]
        public async Task<ActionResult> RequestToJoined(int activityId, string activityCreatedUserId)
        {
            /* var obj = _map.Map<ActivityAttendee>(dto);*/
            
            var user = await _userManager.GetUserAsync(User);

            /*// Ensure if the activityCreatedUserId is the same to ActivityUserId.
            var activityUser = await _context.Activities
                .Include(w=> w.ActivityUser)
                .FirstOrDefaultAsync(w=> w.ActivityUserId == activityCreatedUserId);

            bool userWhoCreatedActivity = activityUser?.ActivityUserId == activityCreatedUserId;

            var validActivityid = await _context.Activities
                .AnyAsync(w => w.Id == activityId);

            // Check if the user attempts to request AGAIN in activity to beling in that room which should NOT
            // It is valid if the user requests an activity in DIFFERENT room/activityId. 
            var isDuplicateUserActivity = await _context.ActivityAttendees
                .Where(w=> w.UserId == user.Id)
                .AnyAsync(w => w.Id == activityId);

            // Check for refactor.
            if (validActivityid && userWhoCreatedActivity != null && userWhoCreatedActivity == true && isDuplicateUserActivity == false)
            {
                var f = new ActivityAttendee()
                {
                    ActivityId = activityId,
                    DateRequest = DateTime.Now,
                    UserId = user.Id,
                    ActivityCreatedUserId = activityCreatedUserId
                    *//*         ActivitiesCreatorId = dto.ActivityCreatorId*//*
                };

                await _context.ActivityAttendees.AddAsync(f);
                await _context.SaveChangesAsync();
            }
            else
            {
                return NotFound();
            }*/
        
            var activityTitle = await _activityAttendee.PostRequestToJoined(user.Id, activityId, activityCreatedUserId);

            return Ok($"Requesting to the {activityTitle} was success. Created Activity User will now see this.");
        }

        // Painfull operation. What if user always click the button
        // Solution: Disable some time the button. and make an limitation.
        [HttpDelete("{activityId:int}")]
        public async Task<ActionResult> RemoveJoined(int activityId)
        {
            var user = await _userManager.GetUserAsync(User);
            /*            if (IsExist(activityId))
                        {
                            var actor = await _context.ActivityAttendees
                                .Include(w => w.Activity)
                                .Where(w=> w.UserId == user.Id)
                                .FirstOrDefaultAsync(x => x.Id == activityId);

                            // Attendess reverting their request is not allowed when Activity ACCEPTS their equest. 
                            if(actor.IsAccepted == null || actor.IsAccepted == false)
                            {
                                _context.ActivityAttendees.Remove(actor);

                            }
                            else
                            {
                                // Show an error that the users cannot revert their join request.
                            }

                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            return NotFound();
                        }*/

            await _activityAttendee.RemovingRequest(user.Id, activityId);
            /* await fileStorageService.DeleteFile(actor.Picture, containerName);*/
            return NoContent();
        }
        private bool IsExist(int id)
        {
            return _context.ActivityAttendees.Any(w=> w.Id ==id);
        }
    }
}
