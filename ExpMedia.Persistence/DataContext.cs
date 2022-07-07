using ExpMedia.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
 
namespace ExpMedia.Persistence
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions<DataContext> options)
        : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            //
            builder.Entity<SharingActivity>()
                .HasOne(u => u.Activity)
                .WithMany(a => a.SharingActivities)
                .HasForeignKey(aa => aa.ActivityId);

            builder.Entity<Activity>()
             .HasOne(u => u.ActivityUser)
             .WithMany(a => a.ActivityUsers)
             .HasForeignKey(aa => aa.ActivityUserId);

            //
            builder.Entity<ActivityUserSelection>()
                .HasOne(u => u.Activity)
                .WithMany(a => a.ActivityUserSelections)
                .HasForeignKey(aa => aa.ActivityId);
            //
            builder.Entity<Comment>()
                .HasOne(b => b.Activity)
                .WithMany(i => i.Comments)
                .HasForeignKey(aa => aa.ActivityId);
            //
            builder.Entity<CommentReaction>()
                .HasOne(b => b.Comment)
                .WithMany(i => i.CommentReactions)
                .HasForeignKey(aa => aa.CommentId);

            // Notif
            builder.Entity<ActivityNotification>()
            .HasOne(b => b.Activity)
            .WithMany(i => i.ActivityNotifications)
            .HasForeignKey(aa => aa.ActivityId);

            builder.Entity<Comment>()
            .HasOne(b => b.Author)
            .WithMany(i => i.AuthorUsers)
            .HasForeignKey(aa => aa.AuthorId);

            builder.Entity<CommentReaction>()
            .HasOne(b => b.CommentCreatedUser)
            .WithMany(i => i.CommentUser)
            .HasForeignKey(aa => aa.CommentCreatedUserId);



            /*            builder.Entity<SharingActivity>(b =>
                        {
                            b.HasKey(k => new { k.ActivityId });
                            b.HasOne(o => o.Activity)
                                .WithMany(f => f.SharingActivities)
                                .HasForeignKey(o => o.ActivityId)
                                .OnDelete(DeleteBehavior.Cascade);
                        });*/
            builder.Entity<UserFollowing>(b =>
            {
                /*b.HasKey(k => new { k.UserToFollowId });*/

                /*                b.HasOne(o => o.UserWhoFollowed)
                                    .WithMany(f => f.Followers)
                                    .HasForeignKey(o => o.UserWhoFollowedId)
                                    .OnDelete(DeleteBehavior.Cascade);*/

                b.HasOne(o => o.UserToFollow)
                    .WithMany(f => f.Followings)
                    .HasForeignKey(o => o.UserToFollowId);
            });
            builder.Entity<TagUser>(b =>
            {
                b.HasOne(o => o.UserToTag)
                    .WithMany(f => f.UsersTagged)
                    .HasForeignKey(o => o.UserToTagId);
            });

            builder.Entity<ActivityNotification>(b =>
            {
                b.HasOne(o => o.NotifyTo)
                    .WithMany(f => f.NotifyToUser)
                    .HasForeignKey(o => o.NotifyToId);
            });

            builder.Entity<TagUser>()
               .HasOne(b => b.Activity)
               .WithMany(i => i.TagUsers)
               .HasForeignKey(aa => aa.ActivityId);

            builder.Entity<BlockUsers>()
            .HasOne(b => b.UserToBlock)
            .WithMany(i => i.ListOfToBlockUser)
            .HasForeignKey(aa => aa.UserToBlockId);

/*            builder.Entity<Messages>(b =>
            {
                b.HasOne(o => o.MessageToUser)
                    .WithMany(f => f.MessageToUsers)
                    .HasForeignKey(o => o.MessageToUserId);
            });*/

            base.OnModelCreating(builder);

        }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<AppUser> AppUser { get; set; }
        public DbSet<ActivityAttendee> ActivityAttendees { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentReaction> CommentReactions { get; set; }
        public DbSet<UserFollowing> UserFollowings { get; set; }
        public DbSet<ActivityUserSelection> ActivityUserSelections { get; set; }
        public DbSet<SharingActivity> SharingActivities { get; set; }
        public DbSet<BlockUsers> BlockUsersx { get; set; }
        public DbSet<TagUser> TagUsers { get; set; }
        public DbSet<ActivityNotification> ActivityNotifications { get; set; }
        public DbSet<Messages> Messagesx { get; set; }
        public DbSet<MessageTable> MessageTables { get; set; }
        public DbSet<MessagesGroup> MessagesGroups { get; set; }
        public DbSet<SubMessageGroup> SubMessageGroups { get; set; }
        public DbSet<SubUserMessages> SubUserMessages { get; set; }
    }
}
