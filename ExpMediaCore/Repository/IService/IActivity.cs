using ExpMedia.Application.ActivitiyFolder;
using ExpMedia.Domain;
using ExpMediaCore.Base;
using ExpMediaCore.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.Repository.IService
{
    public interface IActivity : IGenericRepositoryService<Activity>
    {
        Task PostActivity(ActivityCreationDTO dto, string userId);
        Task RejectingAttendees(string userId, int activityAttendeeId);      
        Task DeleteActivityAttendee(ActivityAttendee obj);
        Task AcceptingAttendies(string userId, int activityAttendeeId);
        Task<ActivityDTO> GetSingleActivity(string userId, int activityId);
        Task<bool> IsActivityAttendeeExists(int activityAttendeesId);
        Task PutActivity(ActivityCreationDTO dto, int activityId);
        Task DeleteActivity(int activityId);

        Task<ActivityAndNotifView> GetFeedActivities(string userId);

    }
}
