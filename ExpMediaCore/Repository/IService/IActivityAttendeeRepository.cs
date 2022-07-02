using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.Repository.IService
{
    public interface IActivityAttendeeRepository
    {
        Task<string> PostRequestToJoined(string userId, int activityId, string activityCreatedUserId);
        Task RemovingRequest(string userId, int activityId);

    }
}
