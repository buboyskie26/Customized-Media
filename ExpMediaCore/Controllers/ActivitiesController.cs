using AutoMapper;
using ExpMedia.Application.Activities;
using ExpMedia.Application.ActivitiyFolder;
using ExpMedia.Domain;
using ExpMedia.Persistence;
using ExpMediaCore.Base;
using ExpMediaCore.Repository.IService;
using MediatR;
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
    public class ActivitiesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _map;
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IActivity _activity;

        public ActivitiesController(IMediator mediator, DataContext context,
            IMapper map, UserManager<AppUser> userManager, IActivity activity)
        {
            this._mediator = mediator;
            _context = context;
            _map = map;
            _userManager = userManager;
            _activity = activity;
        }

        // User who followed other user needs to view their post in the activity feed.
        // User who ddidnt followed other user wont be shown in the activity feed
        [HttpGet]
        public async Task<ActionResult<ActivityAndNotifView>> GetAllActivities()
        {
            var user = await _userManager.GetUserAsync(User);

            // List of user who followed by the user
            var usersHadFollowed = _context.UserFollowings.Where(w => w.UserWhoFollowedId == user.Id)
                .Select(w => w.UserToFollowId).Distinct();

            var useWhoFollowed = _context.UserFollowings.Where(w => w.UserWhoFollowedId == user.Id)
                .Select(w => w.UserWhoFollowedId).Distinct();

            // Id of selected user that included only to see his post
            var userThatIncludesMeMyId = _context.ActivityUserSelections
                .Where(w => w.UsersId == user.Id)
                .Select(w => w.UsersId);

            // Id of the user who selected other users to see his post.
            var userThatIncludesMeHisId = _context.ActivityUserSelections
                .Where(w => w.UsersId == user.Id)
                .Select(w => w.CreatedUserId);

            bool myIdIncludedByOthers = userThatIncludesMeMyId.Contains(user.Id);

            var usersFollowersAndWhoFollowedId = usersHadFollowed.Union(useWhoFollowed);

            var myIdAndOtherUserIdThatIncludesMe = new List<string>();

            var ps = userThatIncludesMeHisId.Union(userThatIncludesMeMyId);

            var sharedPost = _context.SharingActivities
                .Where(w => w.SharedUserId == user.Id)
                .Select(w => w.SharedUserId);

            bool doesISharedPosts = sharedPost.Contains(user.Id);

            var sharedPostPublic = _context.SharingActivities
                .Where(w => w.SharedUserId == user.Id)
                .Where(w => w.OnlyMe == false)
                .Select(w => w.SharedUserId);

            bool doesISharedPostsPublic = sharedPostPublic.Contains(user.Id);

            // User who created the activity must see the user who have sent a request to join
            // in the activity.
            var activityNotif = await (from p in _context.ActivityNotifications
                                       where p.NotifyToId == user.Id
                                       select new ActivityNotificationView()
                                       {
                                           ActivityId = p.ActivityId,
                                           RequestFromId = p.NotifyFromId,
                                           RequestFromName = p.NotifyFrom.FirstName + " " + p.NotifyFrom.LastName
                                       }).AsNoTracking().ToListAsync();

            activityNotif = activityNotif.GroupBy(w => w.ActivityId).Select(w => w.First()).ToList();

            var obj = new List<ActivityView>();

            // Check if you followed someone
            var ifIFollowedSomeone = useWhoFollowed.Contains(user.Id);

            // if you dont have followed someone, only your posts will be given.
            // and your id doesnt include you to the post of another user

            //0
            if (ifIFollowedSomeone == false && myIdIncludedByOthers == false)
            {
                obj = await (from a in _context.Activities

                                 /*  where union.Contains(ty.ActivityUserId)
                                 where t.UserWhoFollowedId == user.Id*/
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
                                 IsOnlyMe = a.OnlyMe == true,
                                 Shares = a.SharingActivities.Count,

                                 IsPostSelected = a.IsSelectedPost == true,

                                 Attendees = (from at in a.Attendees
                                              select new Attendee
                                              {
                                                  Id = at.Id,
                                                  ActivityId = at.ActivityId,
                                                  DateJoined = at.DateJoined,
                                                  UserId = at.UserId
                                              }).ToList(),

                                 SharePosts = a.SharingActivities.Any() ?

                                        (from p in a.SharingActivities


                                         select new ShareActivityView
                                         {
                                             ActivityId = p.ActivityId,
                                             DateOfActivity = p.Activity.DateOfActivity,
                                             Category = p.Activity.Category,
                                             City = p.Activity.City,
                                             Title = p.Activity.Title,
                                             Description = p.Activity.Description,
                                             Venue = p.Activity.Venue,
                                             DateShared = p.DateShared,
                                             UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                             UserSharedPostId = p.SharedUserId,
                                             IsOnlyMe = p.OnlyMe == true
                                         }).ToList() : null
                             }).AsNoTracking().OrderBy(e => e.DateCreation).ToListAsync();
            }
            // 1
            else if (ifIFollowedSomeone == true && myIdIncludedByOthers == false)
            {
                // Check if the user who sets their shared post into only me
                // that user who created the posts could be only seen his only me shared post.

                if (doesISharedPosts == true)
                {
                    // Results is the your only me shared posts 
                    var mysharingPost = (from t in _context.SharingActivities
                                         join a in _context.Activities on t.ActivityId equals a.Id
                                         where t.OnlyMe == true
                                         where t.SharedUserId == user.Id
                                         select new ActivityView()
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
                                             IsOnlyMe = a.OnlyMe == true,
                                             DateCreation = a.DateCreated,
                                             IsPostSelected = a.IsSelectedPost == true,
                                             Shares = a.SharingActivities.Count(),

                                             Attendees = (from at in a.Attendees
                                                          select new Attendee
                                                          {
                                                              Id = at.Id,
                                                              ActivityId = at.ActivityId,
                                                              DateJoined = at.DateJoined,
                                                              UserId = at.UserId
                                                          }).ToList(),

                                             SharePosts = a.SharingActivities.Any() ?

                                                 (from p in a.SharingActivities
                                                  select new ShareActivityView
                                                  {
                                                      ActivityId = p.ActivityId,
                                                      DateOfActivity = p.Activity.DateOfActivity,
                                                      Category = p.Activity.Category,
                                                      City = p.Activity.City,
                                                      Title = p.Activity.Title,
                                                      Description = p.Activity.Description,
                                                      Venue = p.Activity.Venue,
                                                      DateShared = p.DateShared,
                                                      UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                      UserSharedPostId = p.SharedUserId,
                                                      IsOnlyMe = p.OnlyMe == true
                                                  }).ToList() : null
                                         }).ToList();

                    // All PUBLIC shared only post 
                    obj = (from a in _context.Activities

                           where usersFollowersAndWhoFollowedId.Contains(a.ActivityUserId)

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
                               IsOnlyMe = a.OnlyMe == true,
                               DateCreation = a.DateCreated,
                               IsPostSelected = a.IsSelectedPost == true,
                               Shares = a.SharingActivities.Count(),

                               Attendees = (from at in a.Attendees
                                            select new Attendee
                                            {
                                                Id = at.Id,
                                                ActivityId = at.ActivityId,
                                                DateJoined = at.DateJoined,
                                                UserId = at.UserId
                                            }).ToList(),

                               SharePosts = a.SharingActivities.Any() ?

                                            (from p in a.SharingActivities
                                             where p.OnlyMe == false
                                             select new ShareActivityView
                                             {
                                                 ActivityId = p.ActivityId,
                                                 DateOfActivity = p.Activity.DateOfActivity,
                                                 Category = p.Activity.Category,
                                                 City = p.Activity.City,
                                                 Title = p.Activity.Title,
                                                 Description = p.Activity.Description,
                                                 Venue = p.Activity.Venue,
                                                 DateShared = p.DateShared,
                                                 UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                 UserSharedPostId = p.SharedUserId,
                                                 IsOnlyMe = p.OnlyMe == true
                                             }).ToList() : null
                           }).ToList();

                    /*obj = excludedMySharedOnlyPostCopy.ToList();*/

                    // 3, 4
                    var sad = mysharingPost.Select(w => w.Id).ToList();

                    var ind = obj.ToList().FindIndex(a => sad.Contains(a.Id));

                    /*                    var indexes = obj.ToList().FindAll(a=> sad.Contains(a.Id));

                                        var ids = obj.Where(w => sad.Contains(w.Id)).ToList();*/

                    /*obj.RemoveAt(ind);*/
                    var test = obj.Where(e => sad.Contains(e.Id) == false);

                    var e = test.Concat(mysharingPost).ToList();
                    obj = e.OrderByDescending(q => q.ActivityUserName).Distinct().ToList();
                }
                else if (doesISharedPosts == false)
                {
                    obj = (from a in _context.Activities

                           where usersFollowersAndWhoFollowedId.Contains(a.ActivityUserId)

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
                               IsOnlyMe = a.OnlyMe == true,
                               DateCreation = a.DateCreated,
                               IsPostSelected = a.IsSelectedPost == true,
                               Shares = a.SharingActivities.Count(),

                               Attendees = (from at in a.Attendees
                                            select new Attendee
                                            {
                                                Id = at.Id,
                                                ActivityId = at.ActivityId,
                                                DateJoined = at.DateJoined,
                                                UserId = at.UserId
                                            }).ToList(),

                               SharePosts = a.SharingActivities.Any() ?

                                            (from p in a.SharingActivities

                                             where p.OnlyMe == false
                                             select new ShareActivityView
                                             {
                                                 ActivityId = p.ActivityId,
                                                 DateOfActivity = p.Activity.DateOfActivity,
                                                 Category = p.Activity.Category,
                                                 City = p.Activity.City,
                                                 Title = p.Activity.Title,
                                                 Description = p.Activity.Description,
                                                 Venue = p.Activity.Venue,
                                                 DateShared = p.DateShared,
                                                 UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                 UserSharedPostId = p.SharedUserId,
                                                 IsOnlyMe = p.OnlyMe == true
                                             }).ToList() : null
                           }).ToList();
                }
            }
            // 2
            else if (ifIFollowedSomeone == false && myIdIncludedByOthers == true)
            {
                // Single Post came from other users which the loginUser includes his id to see post
                if (doesISharedPosts == true)
                {

                    // Private Post belong to the shared user
                    var mysharingPrivatePost = (from t in _context.SharingActivities
                                                join a in _context.Activities on t.ActivityId equals a.Id
                                                where t.OnlyMe == true
                                                where t.SharedUserId == user.Id
                                                select new ActivityView()
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
                                                    IsOnlyMe = a.OnlyMe == true,
                                                    DateCreation = a.DateCreated,
                                                    IsPostSelected = a.IsSelectedPost == true,
                                                    Shares = a.SharingActivities.Count(),

                                                    Attendees = (from at in a.Attendees
                                                                 select new Attendee
                                                                 {
                                                                     Id = at.Id,
                                                                     ActivityId = at.ActivityId,
                                                                     DateJoined = at.DateJoined,
                                                                     UserId = at.UserId
                                                                 }).ToList(),

                                                    SharePosts = a.SharingActivities.Any() ?

                                                        (from p in a.SharingActivities
                                                         where p.SharedUserId == user.Id
                                                         select new ShareActivityView
                                                         {
                                                             ActivityId = p.ActivityId,
                                                             DateOfActivity = p.Activity.DateOfActivity,
                                                             Category = p.Activity.Category,
                                                             City = p.Activity.City,
                                                             Title = p.Activity.Title,
                                                             Description = p.Activity.Description,
                                                             Venue = p.Activity.Venue,
                                                             DateShared = p.DateShared,
                                                             UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                             UserSharedPostId = p.SharedUserId,
                                                             IsOnlyMe = p.OnlyMe == true
                                                         }).ToList() : null
                                                }).ToList();

                    // Public Post belong to the shared user
                    var publicSharedPosts = (from t in _context.SharingActivities
                                             join a in _context.Activities on t.ActivityId equals a.Id
                                             where t.OnlyMe == false
                                             select new ActivityView()
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
                                                 IsOnlyMe = a.OnlyMe == true,
                                                 DateCreation = a.DateCreated,
                                                 IsPostSelected = a.IsSelectedPost == true,
                                                 Shares = a.SharingActivities.Count(),

                                                 Attendees = (from at in a.Attendees
                                                              select new Attendee
                                                              {
                                                                  Id = at.Id,
                                                                  ActivityId = at.ActivityId,
                                                                  DateJoined = at.DateJoined,
                                                                  UserId = at.UserId
                                                              }).ToList(),

                                                 SharePosts = a.SharingActivities.Any() ?

                                                     (from p in a.SharingActivities
                                                      where p.OnlyMe == false
                                                      select new ShareActivityView
                                                      {
                                                          ActivityId = p.ActivityId,
                                                          DateOfActivity = p.Activity.DateOfActivity,
                                                          Category = p.Activity.Category,
                                                          City = p.Activity.City,
                                                          Title = p.Activity.Title,
                                                          Description = p.Activity.Description,
                                                          Venue = p.Activity.Venue,
                                                          DateShared = p.DateShared,
                                                          UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                          UserSharedPostId = p.SharedUserId,
                                                          IsOnlyMe = p.OnlyMe == true
                                                      }).ToList() : null
                                             }).ToList();

                    // All post that includes me to their post. 
                    var postFromOtherUser = (from a in _context.Activities
                                             join ad in _context.ActivityUserSelections on a.Id equals ad.ActivityId
                                             where ps.Contains(a.ActivityUserId)
                                             where a.IsSelectedPost == true
                                             where ad.UsersId == user.Id
                                             where ps.Contains(a.ActivityUserId)

                                             select new ActivityView
                                             {
                                                 Id = a.Id,
                                                 Shares = a.SharingActivities.Count(),

                                                 DateOfActivity = a.DateOfActivity,
                                                 Category = a.Category,
                                                 City = a.City,
                                                 Venue = a.Venue,
                                                 IsCancelled = a.IsCancelled,
                                                 Title = a.Title,
                                                 Description = a.Description,
                                                 ActivityUserName = a.ActivityUser.FirstName + " " + a.ActivityUser.LastName,
                                                 IsOnlyMe = a.OnlyMe == true,
                                                 DateCreation = a.DateCreated,
                                                 IsPostSelected = a.IsSelectedPost == true,

                                                 Attendees = (from at in a.Attendees
                                                              select new Attendee
                                                              {
                                                                  Id = at.Id,
                                                                  ActivityId = at.ActivityId,
                                                                  DateJoined = at.DateJoined,
                                                                  UserId = at.UserId
                                                              }).ToList(),
                                                 SharePosts = a.SharingActivities.Any() ?

                                                      (from p in a.SharingActivities

                                                       where p.OnlyMe == false
                                                       select new ShareActivityView
                                                       {
                                                           ActivityId = p.ActivityId,
                                                           DateOfActivity = p.Activity.DateOfActivity,
                                                           Category = p.Activity.Category,
                                                           City = p.Activity.City,
                                                           Title = p.Activity.Title,
                                                           Description = p.Activity.Description,
                                                           Venue = p.Activity.Venue,
                                                           DateShared = p.DateShared,
                                                           UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                           UserSharedPostId = p.SharedUserId,
                                                           IsOnlyMe = p.OnlyMe == true
                                                       }).ToList() : null
                                             }).ToList();

                    var myPost = (from a in _context.Activities

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
                                      IsOnlyMe = a.OnlyMe == true,
                                      IsPostSelected = a.IsSelectedPost == true,
                                      DateCreation = a.DateCreated,
                                      Attendees = (from at in a.Attendees
                                                   select new Attendee
                                                   {
                                                       Id = at.Id,
                                                       ActivityId = at.ActivityId,
                                                       DateJoined = at.DateJoined,
                                                       UserId = at.UserId
                                                   }).ToList(),

                                      Shares = a.SharingActivities.Count(),
                                      SharePosts = a.SharingActivities.Any() ?

                                        (from p in a.SharingActivities

                                         where p.OnlyMe == false
                                         select new ShareActivityView
                                         {
                                             ActivityId = p.ActivityId,
                                             DateOfActivity = p.Activity.DateOfActivity,
                                             Category = p.Activity.Category,
                                             City = p.Activity.City,
                                             Title = p.Activity.Title,
                                             Description = p.Activity.Description,
                                             Venue = p.Activity.Venue,
                                             DateShared = p.DateShared,
                                             UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                             UserSharedPostId = p.SharedUserId,
                                             IsOnlyMe = p.OnlyMe == true
                                         }).ToList() : null
                                  }).ToList();

                    // does share post have an public post.
                    var sharedPostContainsPublic = _context.SharingActivities.Any(w => w.OnlyMe == false);

                    if (sharedPostContainsPublic == true)
                    {
                        // If you`re the shared private post user,
                        // you need to see your privte post and other PUBLIC post


                        var ISharedTHePrivatePost = mysharingPrivatePost.Select(w => w.SharePosts.Any(w => w.UserSharedPostId == user.Id));

                        // Logic in the view:
                        // When the user who shared a PRIVATE post (activity)
                        // His private post will be shown together with the shared PUBLIC post on his view
                        // Note that only user who doesnt followed anyone and included his name from the other users post

                        if (ISharedTHePrivatePost.Any())
                        {
                            var pl = mysharingPrivatePost;

                            var kd = mysharingPrivatePost.Concat(publicSharedPosts).Distinct().ToList();

                            var easypc = kd.GroupBy(w => w.Id).Select(w => w.First()).ToList();

                            var nothingFollowedSomeone = mysharingPrivatePost.Concat(myPost).Concat(publicSharedPosts);

                            obj = nothingFollowedSomeone.ToList();
                        }
                        else
                        {
                            var nothingFollowedSomeone = publicSharedPosts.Concat(myPost);
                            obj = nothingFollowedSomeone.GroupBy(e => e.Id).Select(w => w.First()).ToList();
                        }
                    }
                    else if (sharedPostContainsPublic == false)
                    {
                        // If you`re dont have a private shared post
                        // All of the user who posted their private post must be hidden to your view.

                        var nothingFollowedSomeone = publicSharedPosts.Concat(myPost);
                        obj = nothingFollowedSomeone.Distinct().ToList();
                    }

                }
                else if (doesISharedPosts == false)
                {
                    var posting = (from a in _context.Activities
                                   join ad in _context.ActivityUserSelections on a.Id equals ad.ActivityId
                                   where ps.Contains(a.ActivityUserId)
                                   where a.IsSelectedPost == true
                                   where ad.UsersId == user.Id
                                   where ps.Contains(a.ActivityUserId)

                                   select new ActivityView
                                   {
                                       Id = a.Id,
                                       Shares = a.SharingActivities.Count(),

                                       DateOfActivity = a.DateOfActivity,
                                       Category = a.Category,
                                       City = a.City,
                                       Venue = a.Venue,
                                       IsCancelled = a.IsCancelled,
                                       Title = a.Title,
                                       Description = a.Description,
                                       ActivityUserName = a.ActivityUser.FirstName + " " + a.ActivityUser.LastName,
                                       IsOnlyMe = a.OnlyMe == true,
                                       DateCreation = a.DateCreated,
                                       IsPostSelected = a.IsSelectedPost == true,

                                       Attendees = (from at in a.Attendees
                                                    select new Attendee
                                                    {
                                                        Id = at.Id,
                                                        ActivityId = at.ActivityId,
                                                        DateJoined = at.DateJoined,
                                                        UserId = at.UserId
                                                    }).ToList(),
                                       SharePosts = a.SharingActivities.Any() ?

                                            (from p in a.SharingActivities


                                             select new ShareActivityView
                                             {
                                                 ActivityId = p.ActivityId,
                                                 DateOfActivity = p.Activity.DateOfActivity,
                                                 Category = p.Activity.Category,
                                                 City = p.Activity.City,
                                                 Title = p.Activity.Title,
                                                 Description = p.Activity.Description,
                                                 Venue = p.Activity.Venue,
                                                 DateShared = p.DateShared,
                                                 UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                 UserSharedPostId = p.SharedUserId,
                                                 IsOnlyMe = p.OnlyMe == true
                                             }).ToList() : null
                                   }).ToList();

                    var myPost = (from a in _context.Activities

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
                                      IsOnlyMe = a.OnlyMe == true,
                                      IsPostSelected = a.IsSelectedPost == true,
                                      DateCreation = a.DateCreated,
                                      Attendees = (from at in a.Attendees
                                                   select new Attendee
                                                   {
                                                       Id = at.Id,
                                                       ActivityId = at.ActivityId,
                                                       DateJoined = at.DateJoined,
                                                       UserId = at.UserId
                                                   }).ToList(),

                                      Shares = a.SharingActivities.Count(),
                                      SharePosts = a.SharingActivities.Any() ?

                                        (from p in a.SharingActivities

                                         where p.OnlyMe == false
                                         select new ShareActivityView
                                         {
                                             ActivityId = p.ActivityId,
                                             DateOfActivity = p.Activity.DateOfActivity,
                                             Category = p.Activity.Category,
                                             City = p.Activity.City,
                                             Title = p.Activity.Title,
                                             Description = p.Activity.Description,
                                             Venue = p.Activity.Venue,
                                             DateShared = p.DateShared,
                                             UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                             UserSharedPostId = p.SharedUserId,
                                             IsOnlyMe = p.OnlyMe == true
                                         }).ToList() : null
                                  }).ToList();

                    var nothingFollowedSomeone = posting.Concat(myPost);
                    obj = nothingFollowedSomeone.ToList();
                }
            }
            // 3
            else if (ifIFollowedSomeone == true && myIdIncludedByOthers == true)
            {
                // All post that includes me to their post. 
                var postFromUserWhoIncludedMe = (from a in _context.Activities
                                                 join ad in _context.ActivityUserSelections on a.Id equals ad.ActivityId
                                                 where ps.Contains(a.ActivityUserId)
                                                 where a.IsSelectedPost == true
                                                 where ad.UsersId == user.Id
                                                 where ps.Contains(a.ActivityUserId)
                                                 select new ActivityView
                                                 {
                                                     Id = a.Id,
                                                     Shares = a.SharingActivities.Count(),

                                                     DateOfActivity = a.DateOfActivity,
                                                     Category = a.Category,
                                                     City = a.City,
                                                     Venue = a.Venue,
                                                     IsCancelled = a.IsCancelled,
                                                     Title = a.Title,
                                                     Description = a.Description,
                                                     ActivityUserName = a.ActivityUser.FirstName + " " + a.ActivityUser.LastName,
                                                     IsOnlyMe = a.OnlyMe == true,
                                                     DateCreation = a.DateCreated,
                                                     IsPostSelected = a.IsSelectedPost == true,

                                                     Attendees = (from at in a.Attendees
                                                                  select new Attendee
                                                                  {
                                                                      Id = at.Id,
                                                                      ActivityId = at.ActivityId,
                                                                      DateJoined = at.DateJoined,
                                                                      UserId = at.UserId
                                                                  }).ToList(),
                                                     SharePosts = a.SharingActivities.Any() ?

                                                          (from p in a.SharingActivities

                                                           where p.OnlyMe == false
                                                           select new ShareActivityView
                                                           {
                                                               ActivityId = p.ActivityId,
                                                               DateOfActivity = p.Activity.DateOfActivity,
                                                               Category = p.Activity.Category,
                                                               City = p.Activity.City,
                                                               Title = p.Activity.Title,
                                                               Description = p.Activity.Description,
                                                               Venue = p.Activity.Venue,
                                                               DateShared = p.DateShared,
                                                               UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                               UserSharedPostId = p.SharedUserId,
                                                               IsOnlyMe = p.OnlyMe == true
                                                           }).ToList() : null
                                                 }).ToList();



                var postFromUserWhoIncludedMeSamp = (from a in _context.Activities
                                                     join ad in _context.ActivityUserSelections on a.Id equals ad.ActivityId

                                                     where a.IsSelectedPost == true
                                                     where ad.UsersId == user.Id
                                                     where ps.Contains(a.ActivityUserId)

                                                     select new ActivityView
                                                     {
                                                         Id = a.Id,
                                                         Shares = a.SharingActivities.Count(),

                                                         DateOfActivity = a.DateOfActivity,
                                                         Category = a.Category,
                                                         City = a.City,
                                                         Venue = a.Venue,
                                                         IsCancelled = a.IsCancelled,
                                                         Title = a.Title,
                                                         Description = a.Description,
                                                         ActivityUserName = a.ActivityUser.FirstName + " " + a.ActivityUser.LastName,
                                                         IsOnlyMe = a.OnlyMe == true,
                                                         DateCreation = a.DateCreated,
                                                         IsPostSelected = a.IsSelectedPost == true,

                                                         Attendees = (from at in a.Attendees
                                                                      select new Attendee
                                                                      {
                                                                          Id = at.Id,
                                                                          ActivityId = at.ActivityId,
                                                                          DateJoined = at.DateJoined,
                                                                          UserId = at.UserId
                                                                      }).ToList(),
                                                         SharePosts = a.SharingActivities.Any() ?

                                                              (from p in a.SharingActivities
                                                               where p.SharedUserId == user.Id
                                                               where p.OnlyMe == true
                                                               select new ShareActivityView
                                                               {
                                                                   ActivityId = p.ActivityId,
                                                                   DateOfActivity = p.Activity.DateOfActivity,
                                                                   Category = p.Activity.Category,
                                                                   City = p.Activity.City,
                                                                   Title = p.Activity.Title,
                                                                   Description = p.Activity.Description,
                                                                   Venue = p.Activity.Venue,
                                                                   DateShared = p.DateShared,
                                                                   UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                                   UserSharedPostId = p.SharedUserId,
                                                                   IsOnlyMe = p.OnlyMe == true
                                                               }).ToList() : null
                                                     }).ToList();

                var postFromUserWhoIHadFollowed = (from a in _context.Activities
                                                   where usersFollowersAndWhoFollowedId.Contains(a.ActivityUserId)
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
                                                       IsOnlyMe = a.OnlyMe == true,
                                                       DateCreation = a.DateCreated,
                                                       IsPostSelected = a.IsSelectedPost == true,
                                                       Attendees = (from at in a.Attendees
                                                                    select new Attendee
                                                                    {
                                                                        Id = at.Id,
                                                                        ActivityId = at.ActivityId,
                                                                        DateJoined = at.DateJoined,
                                                                        UserId = at.UserId
                                                                    }).ToList(),
                                                       Shares = a.SharingActivities.Count(),

                                                       SharePosts = a.SharingActivities.Any() ?

                                                             (from p in a.SharingActivities

                                                                  /*where p.OnlyMe == false*/
                                                              select new ShareActivityView
                                                              {
                                                                  ActivityId = p.ActivityId,
                                                                  DateOfActivity = p.Activity.DateOfActivity,
                                                                  Category = p.Activity.Category,
                                                                  City = p.Activity.City,
                                                                  Title = p.Activity.Title,
                                                                  Description = p.Activity.Description,
                                                                  Venue = p.Activity.Venue,
                                                                  DateShared = p.DateShared,
                                                                  UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                                  UserSharedPostId = p.SharedUserId,
                                                                  IsOnlyMe = p.OnlyMe == true
                                                              }).ToList() : null
                                                   }).ToList();

                var postFromUserWhoIHadFollowedSamp = (from a in _context.Activities
                                                       join t in _context.SharingActivities on a.Id equals t.ActivityId

                                                       where usersFollowersAndWhoFollowedId.Contains(a.ActivityUserId)
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
                                                           IsOnlyMe = a.OnlyMe == true,
                                                           DateCreation = a.DateCreated,
                                                           IsPostSelected = a.IsSelectedPost == true,
                                                           Attendees = (from at in a.Attendees
                                                                        select new Attendee
                                                                        {
                                                                            Id = at.Id,
                                                                            ActivityId = at.ActivityId,
                                                                            DateJoined = at.DateJoined,
                                                                            UserId = at.UserId
                                                                        }).ToList(),
                                                           Shares = a.SharingActivities.Count(),
                                                           SharePosts = a.SharingActivities.Any() ?

                                                                   (from p in a.SharingActivities

                                                                    where p.OnlyMe == true
                                                                    select new ShareActivityView
                                                                    {
                                                                        ActivityId = p.ActivityId,
                                                                        DateOfActivity = p.Activity.DateOfActivity,
                                                                        Category = p.Activity.Category,
                                                                        City = p.Activity.City,
                                                                        Title = p.Activity.Title,
                                                                        Description = p.Activity.Description,
                                                                        Venue = p.Activity.Venue,
                                                                        DateShared = p.DateShared,
                                                                        UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                                        UserSharedPostId = p.SharedUserId,
                                                                        IsOnlyMe = p.OnlyMe == true
                                                                    }).ToList() : null
                                                       }).ToList();

                var myPost = (from a in _context.Activities

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
                                  IsOnlyMe = a.OnlyMe == true,
                                  IsPostSelected = a.IsSelectedPost == true,
                                  DateCreation = a.DateCreated,
                                  Attendees = (from at in a.Attendees
                                               select new Attendee
                                               {
                                                   Id = at.Id,
                                                   ActivityId = at.ActivityId,
                                                   DateJoined = at.DateJoined,
                                                   UserId = at.UserId
                                               }).ToList(),

                                  Shares = a.SharingActivities.Count(),
                                  SharePosts = a.SharingActivities.Any() ?

                                    (from p in a.SharingActivities

                                     where p.OnlyMe == false
                                     select new ShareActivityView
                                     {
                                         ActivityId = p.ActivityId,
                                         DateOfActivity = p.Activity.DateOfActivity,
                                         Category = p.Activity.Category,
                                         City = p.Activity.City,
                                         Title = p.Activity.Title,
                                         Description = p.Activity.Description,
                                         Venue = p.Activity.Venue,
                                         DateShared = p.DateShared,
                                         UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                         UserSharedPostId = p.SharedUserId,
                                         IsOnlyMe = p.OnlyMe == true
                                     }).ToList() : null
                              }).ToList();

                var mysharingPrivatePost = (from t in _context.SharingActivities
                                            join a in _context.Activities on t.ActivityId equals a.Id
                                            where t.OnlyMe == true
                                            where t.SharedUserId == user.Id
                                            select new ActivityView()
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
                                                IsOnlyMe = a.OnlyMe == true,
                                                DateCreation = a.DateCreated,
                                                IsPostSelected = a.IsSelectedPost == true,
                                                Shares = a.SharingActivities.Count(),

                                                Attendees = (from at in a.Attendees
                                                             select new Attendee
                                                             {
                                                                 Id = at.Id,
                                                                 ActivityId = at.ActivityId,
                                                                 DateJoined = at.DateJoined,
                                                                 UserId = at.UserId
                                                             }).ToList(),

                                                SharePosts = a.SharingActivities.Any() ?

                                                    (from p in a.SharingActivities
                                                     where p.SharedUserId == user.Id
                                                     select new ShareActivityView
                                                     {
                                                         ActivityId = p.ActivityId,
                                                         DateOfActivity = p.Activity.DateOfActivity,
                                                         Category = p.Activity.Category,
                                                         City = p.Activity.City,
                                                         Title = p.Activity.Title,
                                                         Description = p.Activity.Description,
                                                         Venue = p.Activity.Venue,
                                                         DateShared = p.DateShared,
                                                         UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                         UserSharedPostId = p.SharedUserId,
                                                         IsOnlyMe = p.OnlyMe == true
                                                     }).ToList() : null
                                            }).ToList();



                // Public Post belong to the shared user
                var publicSharedPosts = (from t in _context.SharingActivities
                                         join a in _context.Activities on t.ActivityId equals a.Id
                                         where t.OnlyMe == false
                                         select new ActivityView()
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
                                             IsOnlyMe = a.OnlyMe == true,
                                             DateCreation = a.DateCreated,
                                             IsPostSelected = a.IsSelectedPost == true,
                                             Shares = a.SharingActivities.Count(),

                                             Attendees = (from at in a.Attendees
                                                          select new Attendee
                                                          {
                                                              Id = at.Id,
                                                              ActivityId = at.ActivityId,
                                                              DateJoined = at.DateJoined,
                                                              UserId = at.UserId
                                                          }).ToList(),

                                             SharePosts = a.SharingActivities.Any() ?

                                                 (from p in a.SharingActivities
                                                  where p.OnlyMe == false
                                                  select new ShareActivityView
                                                  {
                                                      ActivityId = p.ActivityId,
                                                      DateOfActivity = p.Activity.DateOfActivity,
                                                      Category = p.Activity.Category,
                                                      City = p.Activity.City,
                                                      Title = p.Activity.Title,
                                                      Description = p.Activity.Description,
                                                      Venue = p.Activity.Venue,
                                                      DateShared = p.DateShared,
                                                      UserSharedPostName = p.SharedUser.FirstName + " " + p.SharedUser.LastName,
                                                      UserSharedPostId = p.SharedUserId,
                                                      IsOnlyMe = p.OnlyMe == true
                                                  }).ToList() : null
                                         }).ToList();

                var ISharedThePrivatePost = mysharingPrivatePost.Select(w => w.SharePosts
                        .Any(w => w.UserSharedPostId == user.Id && w.IsOnlyMe == true));

                if (ISharedThePrivatePost.Any())
                {
                    var publicAndPrivateIncludesMe = postFromUserWhoIncludedMe.Concat(postFromUserWhoIncludedMeSamp).ToList();
                    var result = postFromUserWhoIHadFollowed.Concat(publicAndPrivateIncludesMe).Distinct().ToList();

                    obj = result.ToList();

                }
                else
                {
                    // All user PUBLIC posts

                    var res = postFromUserWhoIncludedMe.Concat(postFromUserWhoIHadFollowed);
                    obj = res.ToList();
                }


                /*                var asd = postFromUserWhoIncludedMe.Concat(postFromUserWhoIHadFollowed);
                                obj = asd.ToList();*/
            }


            return new ActivityAndNotifView()
            {
                ActivityFeed = obj,
                ActivityNotifications = activityNotif
            };

        }

        // User who followed other user needs to view their post in the activity feed.
        // User who ddidnt followed other user wont be shown in the activity feed
        [HttpGet("feed")]
        public async Task<ActionResult<ActivityAndNotifView>> GetAllActivitiesSamp()
        {
            var user = await _userManager.GetUserAsync(User);
            return await _activity.GetFeedActivities(user.Id);
        }
        [HttpGet("viewActivity/{activityId}")]
        public async Task<ActionResult<ActivityDTO>> GetSingleActivity(int activityId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (await _activity.isExists(activityId))
            {
                return await _activity.GetSingleActivity(user.Id, activityId);

            }
            else
            {
                return NotFound();
            }
        }

        private async Task<ICollection<ActivityUserComment>> BuildUserComments(int activityId, string userid)
        {
            var item = await (from t in _context.Comments
                              where t.ActivityId == activityId
                              select new ActivityUserComment()
                              {
                                  ActivityId = t.ActivityId,
                                  Body = t.Body,
                                  CommentCreated = t.CreatedAt,
                                  Image = t.Image,
                                  UserId = t.AuthorId,
                                  Username = t.Author.FirstName + " " + t.Author.LastName,
                                  IsCommentOwned = t.AuthorId == userid
                              }).AsNoTracking().ToListAsync();
            return item;
        }

        private ICollection<AttendeesRequest> BuildAttendeeRequest(int id)
        {
            var item = (from f in _context.Activities
                        join fr in _context.ActivityAttendees
                        on f.Id equals fr.ActivityId
                        where fr.IsAccepted == null
                        where f.Id == id
                        select new AttendeesRequest()
                        {
                            ActivityAttendeeId = fr.Id,
                            RequestJoin = fr.DateRequest,
                            UserId = fr.UserId,
                            Username = fr.User.FirstName + " " + fr.User.LastName,
                            IsAccepted = fr.IsAccepted
                        }).ToList();
            return item;
        }

        [HttpPut("acceptingAttendees/{activityAttendeeId}")]
        public async Task<ActionResult> AcceptingAttendies(int activityAttendeeId)
        {
            var user = await _userManager.GetUserAsync(User);
            var IsActivityIdExists = await _activity.IsActivityAttendeeExists(activityAttendeeId);

            if (IsActivityIdExists)
            {
                await _activity.AcceptingAttendies(user.Id, activityAttendeeId);
            }
            else
            {
                return NotFound();
            }
            return Ok(" is now a member");
        }

        [HttpDelete("rejectingAttendees/{activityAttendeeId}")]
        public async Task<ActionResult> RejectingAttendees(int activityAttendeeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (await _activity.IsActivityAttendeeExists(activityAttendeeId))
            {
                await _activity.RejectingAttendees(user.Id, activityAttendeeId);
            }
            else
            {
                return NotFound();

            }
            /*            if (IsExist(activityAttendeeId))
                        {
                            // the user whom created the activity must accept and reject the request of the user
                            var obj = await _context.ActivityAttendees
                                .Where(w => w.ActivityCreatedUserId == user.Id)
                                .FirstOrDefaultAsync(w => w.Id == activityAttendeeId);

                            if (obj != null && obj.ActivityCreatedUserId == user.Id && obj.IsAccepted == true)
                            {
                                _context.ActivityAttendees.Remove(obj);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else
                            return NotFound();*/

            return NoContent();
        }

        [HttpPost("creatingActivity")]
        public async Task<ActionResult> PostActivity([FromBody] ActivityCreationDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);
            await _activity.PostActivity(dto, user.Id);
            return NoContent();

        }

        [HttpPut("editActivity/{activityId}")]
        public async Task<ActionResult> PutActivity(int activityId,
            [FromBody] ActivityCreationDTO dto)
        {
            await _activity.PutActivity(dto, activityId);
            return NoContent();

        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteActivity(int id)
        {
            await _activity.DeleteActivity(id);
            /* await fileStorageService.DeleteFile(actor.Picture, containerName);*/
            return NoContent();
        }

        [HttpPut("setPostAsOnlyMe")]
        public async Task<ActionResult> SetPostAsOnlyMe()
        {
            var user = await _userManager.GetUserAsync(User);
            // All post are now only me.
            var r = (from a in _context.Activities
                     where a.ActivityUserId == user.Id
                     where a.OnlyMe == false
                     select new Activity
                     {
                         Id = a.Id,
                         DateOfActivity = a.DateOfActivity,
                         Category = a.Category,
                         City = a.City,
                         Venue = a.Venue,
                         IsCancelled = a.IsCancelled,
                         Title = a.Title,
                         Description = a.Description,
                         DateCreated = a.DateCreated,
                         ActivityUserId = a.ActivityUserId,
                         OnlyMe = true
                     }).ToList();

            _context.Activities.UpdateRange(r);
            await _context.SaveChangesAsync();

            return Ok("All of your post is now Private (Only me)");
        }


        [HttpPost("postActivityUserSelection")]
        public async Task<ActionResult> PostActivityUserSelection(
            [FromForm] ActivityUserCreationDTO d)
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
                     select new ActivityUserSelection()
                     {
                         ActivityId = created.Id,
                         UsersId = t,
                         CreatedUserId = user.Id
                     }).ToList();

            _context.ActivityUserSelections.AddRange(p);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("searchUsers/{userName}")]
        public async Task<ActionResult<List<SearchUserView>>> PostActivityUserSelection(string userName)
        {
            var user = await _userManager.GetUserAsync(User);

            var searchView = new SearchUserView();

            var block = await _context.BlockUsersx
                .Where(e => e.UserWhoBlockId == user.Id)
                .Where(w => w.UserToBlock.FirstName.ToLower().Contains(userName.Trim().ToLower())
                    || w.UserToBlock.LastName.ToLower().Contains(userName.Trim().ToLower()))
                .ToListAsync();

            var ids = block.Select(w => w.UserToBlockId).ToList();

            // All of the who are not blocked by the login user and login user identity will now show to the view.

            var userInfo = string.IsNullOrEmpty(userName) == false ? await _context.AppUser
                .Where(w => w.FirstName.ToLower().Contains(userName.Trim().ToLower())
                || w.LastName.ToLower().Contains(userName.Trim().ToLower()))
                .Where(w => w.FirstName.ToLower().Contains(user.FirstName.ToLower()) == false
                || w.FirstName.ToLower().Contains(user.FirstName.ToLower()) == false)
                .Where(w => ids.Contains(w.Id) == false)
                .Include(w => w.ActivityUsers)
                .Include(w => w.SharingActivitiesUsers).ThenInclude(w => w.Activity)
                .Select(p => new SearchUserView()
                {
                    Username = p.UserName,
                    Bio = p.Bio,
                    UserId = p.Id,
                    ActivityOfUser = (from py in p.ActivityUsers
                                      select new UserActivities()
                                      {
                                          ActivityId = py.Id,
                                          City = py.City,
                                          Title = py.Title,
                                          Venue = py.Venue,
                                          DateOfActivity = py.DateOfActivity,
                                          Category = py.Category,
                                          Description = py.Description
                                      }).ToList(),
                    SharedPostOfUser = (from t in p.SharingActivitiesUsers
                                        select new UserSharedPosts()
                                        {
                                            DateShared = t.DateShared,
                                            SharedPostId = t.Id,
                                            OnlyMe = t.OnlyMe,
                                            SharedPostActivity = (from ty in p.SharingActivitiesUsers.Select(w => w.Activity)
                                                                  select new Sampp
                                                                  {
                                                                      ActivityId = ty.Id,
                                                                      City = ty.City,
                                                                      Title = ty.Title,
                                                                      Venue = ty.Venue,
                                                                      DateOfActivity = ty.DateOfActivity,
                                                                      Category = ty.Category,
                                                                      Description = ty.Description
                                                                  }).ToList()
                                        }).ToList(),
                })
                .ToListAsync() : null;



            return userInfo;
        }
        [HttpGet("memories")]
        public async Task<ActionResult<MemoryView>> Gett()
        {
            var user = await _userManager.GetUserAsync(User);


            var now = DateTime.Now.Day;
            // Get all activities todays
            var activities = await _context.Activities
                .Include(w => w.ActivityUser)
                .Where(w => w.DateCreated.Day == now)
                .Where(w => w.ActivityUserId == user.Id)
                .AsNoTracking()
                .ToListAsync();

            var tagUser = _context.TagUsers
                 .Select(w => w.UserToTagId)

                 .ToList();

            // Act Jana -> Angelica,Virgie 

            var userTags = (from ty in _context.TagUsers
                            select ty.UserToTagId)
                            .Distinct().ToList();

            var userCreatedTags = (from t in _context.Activities
                                   join ty in _context.TagUsers on t.Id equals ty.Id
                                   where ty.UserWhoTaggedId == user.Id
                                   select ty.UserWhoTaggedId).Distinct().ToList();

            var logInUserToTagged = userTags.Where(w => w == user.Id);

            // User who created a tag must see his activity that he`d  made

            var activityWithTagged = await (from p in _context.TagUsers
                                            where p.UserWhoTaggedId == user.Id ||
                                            logInUserToTagged.Contains(p.UserToTagId)

                                            /*where joining.Contains(p.UserToTagId) || joining.Contains(p.UserWhoTaggedId)*/
                                            select new ActivityMemories()
                                            {
                                                Time = getRelativeDateTime(p.Activity.DateCreated),
                                                ActivityId = p.Activity.Id,
                                                DateOfActivity = p.Activity.DateOfActivity,
                                                Category = p.Activity.Category,
                                                City = p.Activity.City,
                                                Title = p.Activity.Title,
                                                Description = p.Activity.Description,
                                                Venue = p.Activity.Venue,

                                                UserInTheTags = (from ty in p.Activity.TagUsers
                                                                 select new TaggedUsers
                                                                 {
                                                                     Username = ty.UserToTag.FirstName + " " + ty.UserToTag.LastName
                                                                 }).ToList(),

                                            }).AsNoTracking().ToListAsync();
            activityWithTagged = activityWithTagged.GroupBy(w => w.ActivityId).Select(w => w.First()).ToList();

            var activityWithoutTagged = await (from p in _context.Activities
                                               where p.ActivityUserId == user.Id
                                               select new OwnActivity
                                               {
                                                   Time = getRelativeDateTime(p.DateCreated),
                                                   ActivityId = p.Id,
                                                   DateOfActivity = p.DateOfActivity,
                                                   Category = p.Category,
                                                   City = p.City,
                                                   Title = p.Title,
                                                   Description = p.Description,
                                                   Venue = p.Venue,
                                               }).AsNoTracking().ToListAsync();

            // All last month past activity  will be shown.
            return new MemoryView
            {
                // ActivityPost that tagged some users
                MemoryActivities = activityWithTagged,
                // Activity that post by user without any tagged
                MyPastActivities = activityWithoutTagged
            };
        }
        public static string getRelativeDateTime(DateTime date)
        {
            TimeSpan ts = DateTime.Now - date;
            if (ts.TotalMinutes < 1)//seconds ago
                return "just now";
            if (ts.TotalHours < 1)//min ago
                return (int)ts.TotalMinutes == 1 ? "1 Minute ago" : (int)ts.TotalMinutes + " Minutes ago";
            if (ts.TotalDays < 1)//hours ago
                return (int)ts.TotalHours == 1 ? "1 Hour ago" : (int)ts.TotalHours + " Hours ago";
            if (ts.TotalDays < 7)//days ago
                return (int)ts.TotalDays == 1 ? "1 Day ago" : (int)ts.TotalDays + " Days ago";
            if (ts.TotalDays < 30.4368)//weeks ago
                return (int)(ts.TotalDays / 7) == 1 ? "1 Week ago" : (int)(ts.TotalDays / 7) + " Weeks ago";
            if (ts.TotalDays < 365.242)//months ago
                return (int)(ts.TotalDays / 30.4368) == 1 ? "1 Month ago" : (int)(ts.TotalDays / 30.4368) + " Months ago";
            //years ago
            return (int)(ts.TotalDays / 365.242) == 1 ? "1 Year ago" : (int)(ts.TotalDays / 365.242) + " Years ago";
        }

        [HttpGet("activityNotificationView")]
        public async Task<ActionResult<SampView>> ActivityNotfication()
        {
            var user = await _userManager.GetUserAsync(User);

            return await _activity.GetActivityNotfication(user.Id);         
        }
    }
}

