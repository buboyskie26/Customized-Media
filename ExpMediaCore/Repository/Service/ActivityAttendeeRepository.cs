using ExpMedia.Domain;
using ExpMedia.Persistence;
using ExpMediaCore.Repository.IService;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.Repository.Service
{
    public class ActivityAttendeeRepository : IActivityAttendeeRepository
    {
        private readonly DataContext _context;

        public ActivityAttendeeRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<string> PostRequestToJoined(string userId, int activityId, string activityCreatedUserId)
        {
            // Ensure if the activityCreatedUserId is the same to ActivityUserId.
            var activityUser = await ActivityUser(activityCreatedUserId);

            bool userWhoCreatedActivity = activityUser?.ActivityUserId == activityCreatedUserId;

            bool validActivityid = await IsActivityIdPresent(activityId);

            // Check if the user attempts to request AGAIN in activity to beling in that room which should NOT
            // It is valid if the user requests an activity in DIFFERENT room/activityId. 
            bool isDuplicateUserActivity = await DuplicateUserActivity(userId, activityId);
            var now = DateTime.Now;
            if (validActivityid && userWhoCreatedActivity != null && userWhoCreatedActivity == true && isDuplicateUserActivity == false)
            {
                var attendee = new ActivityAttendee()
                {
                    ActivityId = activityId,
                    DateRequest = now,
                    UserId = userId,
                    ActivityCreatedUserId = activityCreatedUserId
                };

                await AddActivityAttendee(attendee);

                var actNotif = new ActivityNotification();

                var acti = await _context.Activities
                    .FirstOrDefaultAsync(w => w.Id == attendee.ActivityId);
                
                actNotif.ActivityId = attendee.ActivityId;
                actNotif.NotificationTime = now;
                actNotif.NotifyToId = attendee.Activity.ActivityUserId;
                actNotif.NotifyFromId = userId;

                await _context.ActivityNotifications.AddAsync(actNotif);
                await _context.SaveChangesAsync();

                return acti?.Title;
            }

            return "No title, something went wrong";
        }

        private async Task AddActivityAttendee(ActivityAttendee attendee)
        {
            await _context.ActivityAttendees.AddAsync(attendee);
            await _context.SaveChangesAsync();
        }

        private async Task<bool> DuplicateUserActivity(string userId, int activityId)
        {
            return await _context.ActivityAttendees
                .Where(w => w.UserId == userId)
                .AnyAsync(w => w.Id == activityId);
        }

        private async Task<bool> IsActivityIdPresent(int activityId)
        {
            return await _context.Activities
                .AnyAsync(w => w.Id == activityId);
        }

        private async Task<Activity> ActivityUser(string activityCreatedUserId)
        {
            return await _context.Activities
                .Include(w => w.ActivityUser)
                .FirstOrDefaultAsync(w => w.ActivityUserId == activityCreatedUserId);
        }

        public async Task RemovingRequest(string userId, int activityId)
        {
            if (IsExist(activityId))
            {
                var actor = await _context.ActivityAttendees
                    .Include(w => w.Activity)
                    .Where(w => w.UserId == userId)
                    .FirstOrDefaultAsync(x => x.Id == activityId);

                // Attendess reverting their request is not allowed when Activity ACCEPTS their equest. 
                if (actor.IsAccepted == null || actor.IsAccepted == false)
                {
                    _context.ActivityAttendees.Remove(actor);

                }
                else
                {
                    // Show an error that the users cannot revert their join request.
                }

                await _context.SaveChangesAsync();
            }
        }
        private bool IsExist(int id)
        {
            return _context.ActivityAttendees.Any(w => w.Id == id);
        }
    }
}
