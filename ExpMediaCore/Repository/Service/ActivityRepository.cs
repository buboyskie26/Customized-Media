using AutoMapper;
using ExpMedia.Application.ActivitiyFolder;
using ExpMedia.Domain;
using ExpMedia.Persistence;
using ExpMediaCore.Base;
using ExpMediaCore.Repository.IService;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.Repository.Service
{
/*    public class ActivityRepository : EntityBaseRepository<Activity>, IActivity*/
    public class ActivityRepository : IActivity
    {
        private readonly IMapper _map;

        private readonly DataContext _context;
        public ActivityRepository(DataContext app, IMapper map)
        {
            _context = app;
            _map = map;
        }
 
        public async Task AcceptingAttendies(string userId, int activityAttendeeId)
        {

            var obj = await UserWhoCreatedActivity(userId, activityAttendeeId);
            // the user whom created the activity must accept and reject the request of the user
            if (obj != null && obj.ActivityCreatedUserId == userId && obj.IsAccepted == null)
            {
                obj.DateJoined = DateTime.Now;
                obj.IsAccepted = true;

               await UpdateActivityAttendee(obj);
            }
        }

        public async Task<bool> Create(Activity entity)
        {
            await _context.Activities.AddAsync(entity);
            return await Save();
        }

    

        public async Task DeleteActivityAttendee(ActivityAttendee obj)
        {
            _context.ActivityAttendees.Remove(obj);
            await _context.SaveChangesAsync();
        }

        public Task<ICollection<Activity>> FindAll()
        {
            throw new NotImplementedException();
        }

        public async Task<Activity> FindById(int activityId)
        {
            return await _context.Activities
            .Include(w => w.Comments)
            .FirstOrDefaultAsync(w => w.Id == activityId);

        }

        public async Task<ActivityDTO> GetSingleActivity(string userId, int activityId)
        {
            var activity = await _context.Activities
                .Include(w => w.Comments).ThenInclude(w=> w.CommentReactions).ThenInclude(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == activityId);

            var fd = activity.Comments.Where(w => w.ActivityId == activityId).FirstOrDefault();

            var list = activity.Comments.Where(w => w.ActivityId == activityId).ToList();
            var logInUser = await LoggedInUser(userId, activityId);

            var userWhoCreateActivity = await UserWhoCreateActivity(userId, activityId);

            // Check if the user who are not accepted in the activityatendee cannot saw the usercomment
            // if the creator of activity could see the usercomments, its members and reuest people to enter the room.
            var obj = new ActivityDTO();

            var activityIdExists = await isExists(activityId);

            if (activityIdExists)
            {
                obj = new ActivityDTO
                {
                    Category = activity.Category,
                    City = activity.City,
                    IsCancelled = activity.IsCancelled,
                    DateOfActivity = activity.DateOfActivity,
                    Description = activity.Description,
                    Id = activity.Id,
                    Title = activity.Title,
                    Venue = activity.Venue,

                    /*                AttendeesRequests = activity.ActivityUserId == userId ? BuildAttendeeRequest(activity.Id) : null,*/

                    AttendeesRequests = activity.ActivityUserId == userId ? (from f in _context.Activities
                                                                             join fr in _context.ActivityAttendees
                                                                              on f.Id equals fr.ActivityId
                                                                             where fr.IsAccepted == null
                                                                             where f.Id == activityId
                                                                             select new AttendeesRequest()
                                                                             {
                                                                                 ActivityAttendeeId = fr.Id,
                                                                                 RequestJoin = fr.DateRequest,
                                                                                 UserId = fr.UserId,
                                                                                 Username = fr.User.FirstName + " " + fr.User.LastName,
                                                                                 IsAccepted = fr.IsAccepted
                                                                             }).ToList() : null,
                    AttendeeMembers = activity.ActivityUserId == userId ? (from f in _context.Activities
                                                                           join fr in _context.ActivityAttendees
                                                                           on f.Id equals fr.ActivityId
                                                                           where fr.IsAccepted == true
                                                                           where f.Id == activityId

                                                                           select new AttendeeMember()
                                                                           {
                                                                               ActivityAttendeeId = fr.Id,
                                                                               DateJoined = fr.DateJoined,
                                                                               UserId = fr.UserId,
                                                                               Username = fr.User.FirstName + " " + fr.User.LastName,

                                                                           }).ToList() : null,

                    UserComments = (activity.Comments.Any() && logInUser?.IsAccepted == true)
                    || userWhoCreateActivity?.ActivityCreatedUserId == userId ? await BuildUserComments(activity.Id, userId) : null
                };
            }
            return obj;
        }
 
        private async Task<ICollection<ActivityUserComment>> BuildUserComments(int activityId,
            string userid)
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
                                  IsCommentOwned = t.AuthorId == userid,
                                  CommentId =  t.Id,
                                  LikeCount = t.CommentReactions.Where(w=> w.Like > 0).Count(),
                                  HeartCount = t.CommentReactions.Where(w=> w.Heart > 0).Count(),

                                  /* Like = list.Where(w=> w.Id == t.Id).Select(q=> q.CommentReactions.Select(e=> e.Like)).Count()*/
                                  UserReactionToComments = (from yt in t.CommentReactions
                                                            select new UserReactionToComment()
                                                            {
                                                                Username = yt.User.FirstName + " " + yt.User.LastName,
                                                                CommentReactionId = yt.Id,
                                                                Heart = yt.Heart,
                                                                Like = yt.Like,
                                                                CommentId = t.Id
                                                            }).ToList()
                              }).AsNoTracking().ToListAsync();
            return item;
        }

        public async Task<bool> isExists(int id)
        {
            return await _context.Activities.AnyAsync(w => w.Id == id);
        }

        public async Task PostActivity(ActivityCreationDTO dto,string userId)
        {
            var obj = _map.Map<Activity>(dto);
            obj.DateCreated = DateTime.Now;
            obj.ActivityUserId = userId;
            obj.IsCancelled = false;
            obj.DateOfActivity = obj.DateCreated.AddDays(3);

            await Create(obj);
        }

        public async Task RejectingAttendees(string userId,int activityAttendeeId)
        {
            var obj = await UserWhoCreatedActivity(userId, activityAttendeeId);

            if (obj != null && obj.ActivityCreatedUserId == userId && obj.IsAccepted == true)
            {
                await DeleteActivityAttendee(obj);
            }
        }

        public async Task<bool> Save()
        {
            var changes = await _context.SaveChangesAsync();
            return changes > 0;
        }

 

        public async Task UpdateActivityAttendee(ActivityAttendee obj)
        {
            _context.ActivityAttendees.Update(obj);
            await _context.SaveChangesAsync();
        }

        public async Task<ActivityAttendee> UserWhoCreatedActivity(string userId, int activityAttendeeId)
        {
            return await _context.ActivityAttendees
                    .Where(w => w.ActivityCreatedUserId == userId)
                    .FirstOrDefaultAsync(w => w.Id == activityAttendeeId);
        }

        public async Task<ActivityAttendee> LoggedInUser(string userId, int activityId)
        {
            return  await _context.ActivityAttendees
            .Include(w => w.Activity)
            .Include(w => w.User)
            .Where(w => w.UserId == userId)
            .Where(w => w.ActivityId == activityId)
            .FirstOrDefaultAsync();
        }

        public async Task<ActivityAttendee> UserWhoCreateActivity(string userId, int activityId)
        {
            return await _context.ActivityAttendees
            .Include(w => w.Activity)
            .Include(w => w.User)
            .Where(w => w.ActivityId == activityId)
            .Where(w => w.ActivityCreatedUserId == userId)
            .FirstOrDefaultAsync();
        }

        public async Task<bool> IsActivityAttendeeExists(int activityAttendeesId)
        {
            return await _context.ActivityAttendees.AnyAsync(w => w.Id == activityAttendeesId);

        }

        public async Task PutActivity(ActivityCreationDTO dto, int activityId)
        {
            var obj = await FindActivityId(activityId);
            obj = _map.Map(dto, obj);

            await UpdateActivity(obj);
        }

        private async Task UpdateActivity(Activity obj)
        {
            _context.Activities.Update(obj);
            await _context.SaveChangesAsync();
        }

        private async Task<Activity> FindActivityId(int activityId)
        {
            return await _context.Activities.FirstOrDefaultAsync(w => w.Id == activityId);
        }

        public Task Update(Activity entity)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteActivity(int activityId)
        {
          
            var actor = await FindById(activityId);
            if (actor != null)
            {
                _context.Remove(actor);
                await _context.SaveChangesAsync();
            }

          
        }

        public async Task<ActivityAndNotifView> GetFeedActivities(string userId)
        {
            var usersHadFollowed = _context.UserFollowings.Where(w => w.UserWhoFollowedId == userId)
               .Select(w => w.UserToFollowId).Distinct();

            var useWhoFollowed = _context.UserFollowings.Where(w => w.UserWhoFollowedId == userId)
                .Select(w => w.UserWhoFollowedId).Distinct();

            // Id of selected user that included only to see his post
            var userThatIncludesMeMyId = _context.ActivityUserSelections
                .Where(w => w.UsersId == userId)
                .Select(w => w.UsersId);

            // Id of the user who selected other users to see his post.
            var userThatIncludesMeHisId = _context.ActivityUserSelections
                .Where(w => w.UsersId == userId)
                .Select(w => w.CreatedUserId);

            bool myIdIncludedByOthers = userThatIncludesMeMyId.Contains(userId);

            var usersFollowersAndWhoFollowedId = usersHadFollowed.Union(useWhoFollowed);

            var userIncludesMeHisIdAndMyId = userThatIncludesMeHisId.Union(userThatIncludesMeMyId);

            var sharedPost = _context.SharingActivities
                .Where(w => w.SharedUserId == userId)
                .Select(w => w.SharedUserId);

            bool doesISharedPosts = sharedPost.Contains(userId);

            var sharedPostPublic = _context.SharingActivities
                .Where(w => w.SharedUserId == userId)
                .Where(w => w.OnlyMe == false)
                .Select(w => w.SharedUserId);

            bool doesISharedPostsPublic = sharedPostPublic.Contains(userId);

            // User who created the activity must see the user who have sent a request to join
            // in the activity.
            var activityNotif = await (from p in _context.ActivityNotifications
                                       where p.NotifyToId == userId
                                       select new ActivityNotificationView()
                                       {
                                           ActivityId = p.ActivityId,
                                           RequestFromId = p.NotifyFromId,
                                           RequestFromName = p.NotifyFrom.FirstName + " " + p.NotifyFrom.LastName
                                       }).AsNoTracking().ToListAsync();

            activityNotif = activityNotif.GroupBy(w => w.ActivityId).Select(w => w.First()).ToList();

            var obj = new List<ActivityView>();

            // Check if you followed someone
            var ifIFollowedSomeone = useWhoFollowed.Contains(userId);

            // if you dont have followed someone, only your posts will be given.
            // and your id doesnt include you to the post of another user
            //0 
            if (ifIFollowedSomeone == false && myIdIncludedByOthers == false)
            {
                // if you dont have followed someone, only your posts will be given.
                // and your id doesnt include you to the post of another user
                obj = await AllMyActivityPostOnly(userId, obj);
            }
            // 1
            else if (ifIFollowedSomeone == true && myIdIncludedByOthers == false)
            {
                // Check if the user who sets their shared post into only me
                // that user who created the posts could be only seen his only me shared post.

                if (doesISharedPosts == true)
                {
                    // Results is the your only me shared posts 
                    List<ActivityView> myPrivateSharedPosts = MyOnlyMeSharedPost_Private(userId);

                    // All PUBLIC shared only post 
                    obj = MyPostAndFollowingUserPost_Shared_Public(usersFollowersAndWhoFollowedId);

                    var mySharedPostIds = myPrivateSharedPosts.Select(w => w.Id).ToList();

                    var ind = obj.ToList().FindIndex(a => mySharedPostIds.Contains(a.Id));

                    
                    /* var indexes = obj.ToList().FindAll(a=> mySharedPostIds.Contains(a.Id));
                    var ids = obj.Where(w => mySharedPostIds.Contains(w.Id)).ToList();*/
                    /*obj.RemoveAt(ind);*/

                    var removeMySimilarPost = obj.Where(e => mySharedPostIds.Contains(e.Id) == false);

                    var e = removeMySimilarPost.Concat(myPrivateSharedPosts).ToList();
                    obj = e.OrderByDescending(q => q.ActivityUserName).Distinct().ToList();
                }
                else if (doesISharedPosts == false)
                {
                    // All PUBLIC shared only post 
                    obj = MyPostAndFollowingUserPost_Shared_Public(usersFollowersAndWhoFollowedId);
                }
            }
            // 2
            else if (ifIFollowedSomeone == false && myIdIncludedByOthers == true)
            {
                // Single Post came from other users which the loginUser includes his id to see post
                if (doesISharedPosts == true)
                {
                    // Private Post belong to the shared user
                    // where p.SharedUserId == userId is the difference. to the other one
                    List<ActivityView> myPrivateSharedPosts = MyOnlyMeSharedPost_Private(userId);

                    // Public Post belong to the shared user
                    List<ActivityView> publicSharedPosts = PublicSharedPostsFromUsers();

                    // All post that includes me to their post. 
                    PostFromOtherUser_PublicSharedPost(userId, userIncludesMeHisIdAndMyId);

                    List<ActivityView> myPost = MyPost_PublicSharedPost(userId);

                    // does share post have an public post.
                    var sharedPostContainsPublic = _context.SharingActivities.Any(w => w.OnlyMe == false);

                    if (sharedPostContainsPublic == true)
                    {
                        // If you`re the shared private post user,
                        // you need to see your privte post and other PUBLIC post


                        var ISharedTHePrivatePost = myPrivateSharedPosts.Select(w => w.SharePosts.Any(w => w.UserSharedPostId == userId));

                        var CombiningPublicSharedPostAndMyPost = publicSharedPosts.Concat(myPost);

                        // Logic in the view:
                        // When the user who shared a PRIVATE post (activity)
                        // His private post will be shown together with the shared PUBLIC post on his view
                        // Note that only user who doesnt followed anyone and included his name from the other users post

                        if (ISharedTHePrivatePost.Any())
                        {
                            var myPrivateSharedPostAndPublicSharedPost = myPrivateSharedPosts.Concat(publicSharedPosts).Distinct().ToList();
/*                            var myPrivateSharedPostAndPublicSharedPost = myPrivateSharedPosts.Concat(CombiningPublicSharedPostAndMyPost).Distinct().ToList();
                            obj = myPrivateSharedPostAndPublicSharedPost.ToList();*/

                            var nothingFollowedSomeone = myPrivateSharedPostAndPublicSharedPost.Concat(myPost).ToList();
                            obj = nothingFollowedSomeone.ToList();
                        }
                        else
                        {
                            var nothingFollowedSomeone = CombiningPublicSharedPostAndMyPost;
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
                                   where userIncludesMeHisIdAndMyId.Contains(a.ActivityUserId)
                                   where a.IsSelectedPost == true
                                   where ad.UsersId == userId                         
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

                    var myPost = MyPost_PublicSharedPost(userId);

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
                                                 where userIncludesMeHisIdAndMyId.Contains(a.ActivityUserId)
                                                 where a.IsSelectedPost == true
                                                 where ad.UsersId == userId
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
                                                     where ad.UsersId == userId
                                                     where userIncludesMeHisIdAndMyId.Contains(a.ActivityUserId)

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
                                                               where p.SharedUserId == userId
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

                var myPost = MyPost_PublicSharedPost(userId);

                List<ActivityView> myPrivateSharedPosts = MyPrivateSharedPosts(userId);



                // Public Post belong to the shared user
               
                var publicSharedPosts = PublicSharedPostsFromUsers();

                var ISharedThePrivatePost = myPrivateSharedPosts.Select(w => w.SharePosts
                        .Any(w => w.UserSharedPostId == userId && w.IsOnlyMe == true));

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
            }


            return new ActivityAndNotifView()
            {
                ActivityFeed = obj,
                ActivityNotifications = activityNotif
            };
        }

        private List<ActivityView> MyPrivateSharedPosts(string userId)
        {
            return (from t in _context.SharingActivities
                    join a in _context.Activities on t.ActivityId equals a.Id
                    where t.OnlyMe == true
                    where t.SharedUserId == userId
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
                             where p.SharedUserId == userId
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

        private List<ActivityView> MyPost_PublicSharedPost(string userId)
        {
            return (from a in _context.Activities

                    where a.ActivityUserId == userId
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
        }
        private void PostFromOtherUser_PublicSharedPost(string userId, IQueryable<string> userIncludesMeHisIdAndMyId)
        {
            var postFromOtherUser = (from a in _context.Activities
                                     join ad in _context.ActivityUserSelections on a.Id equals ad.ActivityId
                                     where userIncludesMeHisIdAndMyId.Contains(a.ActivityUserId)
                                     where a.IsSelectedPost == true
                                     where ad.UsersId == userId
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
        }
        private List<ActivityView> PublicSharedPostsFromUsers()
        {
            return (from t in _context.SharingActivities
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
        }
        private List<ActivityView> MyOnlyMeSharedPost_Private(string userId)
        {
            return (from t in _context.SharingActivities
                    join a in _context.Activities on t.ActivityId equals a.Id
                    where t.OnlyMe == true
                    where t.SharedUserId == userId
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
        }
        private List<ActivityView> MyPostAndFollowingUserPost_Shared_Public(IQueryable<string> usersFollowersAndWhoFollowedId)
        {
            return (from a in _context.Activities
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
        private async Task<List<ActivityView>> AllMyActivityPostOnly(string userId, List<ActivityView> obj)
        {
            obj = await (from a in _context.Activities

                             /*  where union.Contains(ty.ActivityUserId)
                             where t.UserWhoFollowedId == userId*/
                         where a.ActivityUserId == userId
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
            return obj;
        }
    }
}
