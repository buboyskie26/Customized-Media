using AutoMapper;
using ExpMedia.Application.CommentFolder;
using ExpMedia.Application.CommentReactionFolder;
using ExpMedia.Application.Helper;
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
    public class CommentController : ControllerBase
    {
        private readonly IMapper _map;
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IFileStorageService fileStorageService;
        private string container = "commentsToActivityAttendee";

        public CommentController(DataContext context, IMapper map,
            UserManager<AppUser> userManager, IFileStorageService fileStorageService)
        {
            _context = context;
            _map = map;
            _userManager = userManager;
            this.fileStorageService = fileStorageService;
        }
        [HttpGet("{commentId}")]
        public async Task<ActionResult<List<CommentDTO>>> GetCommentWithReaction(int commentId)
        {
            var r = new List<CommentDTO>();
            var obj = (from y in _context.Comments
                     join yt in _context.CommentReactions
                     on y.Id equals yt.CommentId
                     where y.Id == commentId
                     select new Reaction
                     {
                         CommentId = yt.CommentId,
                         UserId = yt.UserId,
                         Like = yt.Like,
                         Heart = yt.Heart
                     }).ToList();

            var item = await (from t in _context.Comments

                              where t.Id == commentId
                              select new CommentDTO
                              {
                                ActivityId=t.ActivityId,
                                AuthorId=t.AuthorId,
                                Body=t.Body,
                                CreatedAt=t.CreatedAt,
                                Image=t.Image,
                                HeartCount = t.CommentReactions.Where(w=> w.Heart > 0).Count(),
                                LikeCount = t.CommentReactions.Where(w => w.Like > 0).Count(),
                                Id = t.Id,
                                CommentReactions = BuildReaction(commentId)
                              }).ToListAsync();

            return item;
        }

        private List<Reaction> BuildReaction(int commentId)
        {
            var t = (from y in _context.Comments
                     join yt in _context.CommentReactions
                     on y.Id equals yt.CommentId
                     where y.Id == commentId
                     select new Reaction
                     {
                         CommentId=yt.CommentId,
                         UserId = yt.UserId,
                         Like=yt.Like,
                         Heart=yt.Heart,
                         DateReact=yt.DateReact,
                         Id=yt.Id,
                         Unlike=yt.Unlike
                     }).ToList();
            return t;
        }

        [HttpPost]
        public async Task<ActionResult> CommentOnActivity([FromForm] CommentCreationDTO dto)
        {
            /* var obj = _map.Map<ActivityAttendee>(dto);*/

            var user = await _userManager.GetUserAsync(User);

            // Check the user if they`re accepted in the activityattendee room.
            var attendee = await _context.ActivityAttendees
                .Where(w => w.UserId == user.Id)
                .Where(w => w.IsAccepted == true)
                .AnyAsync(w => w.ActivityId == dto.ActivityId);

            if (attendee)
            {
                var comment = _map.Map<Comment>(dto);
                comment.AuthorId = user.Id;
                if (dto.Image != null)
                {
                    comment.Image = await fileStorageService.SaveFile(container, dto.Image);
                }
                await _context.Comments.AddAsync(comment);
                await _context.SaveChangesAsync();
            }

            return Ok("Successfully commented on activity");
        }

/*        [HttpGet("{commentId:int}")]
        public async Task<ActionResult> PutGetEditCommentOnActivity(int commentId, [FromForm] CommentCreationDTO dto)
        {

        }*/
            [HttpPut("{commentId:int}")]
        public async Task<ActionResult> EditCommentOnActivity(int commentId, [FromForm] CommentCreationDTO dto)
        {
            /* var obj = _map.Map<ActivityAttendee>(dto);*/

            var user = await _userManager.GetUserAsync(User);

            // Check the user if they`re accepted in the activityattendee room.
            var comment = await _context.Comments
                .Where(w => w.AuthorId == user.Id)

                .AnyAsync(w => w.Id == commentId);

            if (comment)
            {
                var commentExists = await _context.Comments
                    .Where(w => w.AuthorId == user.Id)
                    .FirstOrDefaultAsync(w => w.Id == commentId);

                commentExists = _map.Map(dto, commentExists);
                commentExists.AuthorId = user.Id;

                if (dto.Image != null)
                {
                    commentExists.Image = await fileStorageService.EditFile(container, dto.Image, commentExists.Image);
                }

                  _context.Comments.Update(commentExists);
                await _context.SaveChangesAsync();
            }


            return NoContent();
        }

    }
}
