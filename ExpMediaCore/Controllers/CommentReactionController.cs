using AutoMapper;
using ExpMedia.Application.CommentReactionFolder;
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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExpMediaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentReactionController : ControllerBase
    {
        private readonly IMapper _map;
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CommentReactionController(IMapper map, DataContext context, UserManager<AppUser> userManager)
        {
            _map = map;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("likingComment")]
        public async Task<ActionResult> CommentReacting([FromBody] CommentReactionCreationDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);

            // user needs to be accepted before they react to the comments on the activity.
            var activity = await _context.Activities
                .Include(w => w.Comments)
                .FirstOrDefaultAsync(w => w.Id == dto.ActivityId);

            // check if loginUser has been accepted in the activity
            var logInUser =  await _context.ActivityAttendees
                .Include(w => w.Activity)
                .Include(w => w.User)
                .Where(w => w.UserId == user.Id)
                .Where(w => w.IsAccepted == true)
                .Where(w => w.ActivityId == dto.ActivityId)
                .FirstOrDefaultAsync();

            // Check commentid if Exists
            var commentExists = await _context.Comments
              .AnyAsync(w => w.Id == dto.CommentId);

            // check if user already react to the comment and AVOID REACTING AGAIN.
            var alreadyReactsWithSameComment = await _context.CommentReactions
              .Where(w=> w.UserId == user.Id)
            .AnyAsync(w => w.CommentId == dto.CommentId);

            // Get the user owned commentReaction Id that needs to be replace the like into HEART.
            var commentId = await _context.CommentReactions
                .Include(w => w.Comment)
                .Where(w => w.UserId == user.Id)
                 .FirstOrDefaultAsync(w => w.CommentId == dto.CommentId);

            if (commentExists && logInUser != null && logInUser.IsAccepted == true)
            {
                if(alreadyReactsWithSameComment == false)
                {
                    var t = new CommentReaction()
                    {
                        CommentId = dto.CommentId,
                        Like = 1,
                        DateReact = DateTime.Now,
                        UserId = user.Id,
                        CommentCreatedUserId = dto.CommentCreatedUserId
                    };
                    await _context.CommentReactions.AddAsync(t);
                    await _context.SaveChangesAsync();

                    // From heart replacing into like reaction.

                }else if(alreadyReactsWithSameComment == true && commentId.Like == 0)
                {

                    commentId.Like = 1;
                    commentId.Heart = 0;
                    commentId.DateReact = DateTime.Now;
                    commentId.UserId = user.Id;

                    _context.CommentReactions.Update(commentId);
                    await _context.SaveChangesAsync();
                }
                else if (alreadyReactsWithSameComment == true && commentId.Like == 1)
                {
                    // changed the like into heart
                    return BadRequest("You already like the comment.");
                }
                else
                {
                    // Already reacts to the same comment.
                    return BadRequest("Only once could like a post.");
                }
            }
            else
            {
                return BadRequest("You`re not belong to the activity or The comment and post is now deleted..");
            }
            return Ok("Successfully likes the comments");

        }

        [HttpPut("heartingComment")]
        public async Task<ActionResult> HeartingComment([FromBody] CommentReactionCreationDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);

            // user needs to be accepted before they react to the comments on the activity.
            var activity = await _context.Activities
                .Include(w => w.Comments)
                .FirstOrDefaultAsync(w => w.Id == dto.ActivityId);

            // check if loginUser has been accepted in the activity
            var logInUser = await _context.ActivityAttendees
                .Include(w => w.Activity)
                .Include(w => w.User)
                .Where(w => w.UserId == user.Id)
                .Where(w => w.IsAccepted == true)
                .Where(w => w.ActivityId == dto.ActivityId)
                .FirstOrDefaultAsync();

            // Check commentid if Exists
            var commentExists = await _context.Comments
              .AnyAsync(w => w.Id == dto.CommentId);

            // check if user already react to the comment and AVOID REACTING AGAIN.
            var alreadyReactsWithSameComment = await _context.CommentReactions
              .Where(w => w.UserId == user.Id)
              .AnyAsync(w => w.CommentId == dto.CommentId);

            // Get the user owned commentReaction Id that needs to be replace the like into HEART.
            var commentId = await _context.CommentReactions
                .Include(w=> w.Comment)
                .Where(w=> w.UserId == user.Id)
                 .FirstOrDefaultAsync(w => w.CommentId == dto.CommentId);

            if (commentExists && logInUser != null && logInUser.IsAccepted == true)
            {
                // If there`s no id in the commentREaction database 
                // and user wanted to heart straightly rathan than like
                if (alreadyReactsWithSameComment == false)
                {
                    var t = new CommentReaction()
                    {
                        CommentId = dto.CommentId,
                        Heart = 1,
                        DateReact = DateTime.Now,
                        UserId = user.Id,
                        CommentCreatedUserId = dto.CommentCreatedUserId
                    };
                    await _context.CommentReactions.AddAsync(t);
                    await _context.SaveChangesAsync();
                }
                // From heart replacing into like reaction.
                // needs to have an existing react to comment id to change it to another react             
                else if (alreadyReactsWithSameComment == true && commentId.Heart == 0)
                {
                    // changed the like into heart
                    commentId.Like = 0;
                    commentId.Heart = 1;
                    commentId.DateReact = DateTime.Now;
                    commentId.UserId = user.Id;

                    _context.CommentReactions.Update(commentId);
                    await _context.SaveChangesAsync();
                }
                // Check if the user hearts the comment again.
                else if (alreadyReactsWithSameComment == true && commentId.Heart == 1)
                {
                    // changed the like into heart
                    return BadRequest("You already heart the comment.");

                }
                else
                {
                    // Already reacts to the same comment.
                    return BadRequest("Only once could like a post.");
                }
            }
            else
            {
                return BadRequest("You`re not belong to the activity.");
            }
            return Ok("Successfully hearts the comments");


        }

        [HttpPut("unReactingComment/{commentId}")]
        public async Task<ActionResult> UnCommentReacting([FromBody] CommentReactionCreationDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);

            // user needs to be accepted before they react to the comments on the activity.
            var activity = await _context.Activities
                .Include(w => w.Comments)
                .FirstOrDefaultAsync(w => w.Id == dto.ActivityId);

            // check if loginUser has been accepted in the activity
            var logInUser = await _context.ActivityAttendees
                .Include(w => w.Activity)
                .Include(w => w.User)
                .Where(w => w.UserId == user.Id)
                .Where(w => w.IsAccepted == true)
                .Where(w => w.ActivityId == dto.ActivityId)
                .FirstOrDefaultAsync();

            // Check commentid if Exists
            var commentExists = await _context.Comments
              .AnyAsync(w => w.Id == dto.CommentId);

            // check if user already react to the comment and AVOID REACTING AGAIN.
            var alreadyReactsWithSameComment = await _context.CommentReactions
              .Where(w => w.UserId == user.Id)
              .AnyAsync(w => w.CommentId == dto.CommentId);

            // Get the user owned commentReaction Id that needs to be replace the like into HEART.
            var commentId = await _context.CommentReactions
                .Include(w => w.Comment)
                .Where(w => w.UserId == user.Id)
                 .FirstOrDefaultAsync(w => w.CommentId == dto.CommentId);

            if (commentExists && logInUser != null && logInUser.IsAccepted == true)
            {
                // needs to have an existing react to comment id to change it to another react             
                if (alreadyReactsWithSameComment == true && commentId.Like > 0 || commentId.Heart > 0)
                {
                    // Reset the properties into 0
                    // We dont want to delete, this serve as a reference that user has react the comment
                    // even he revert it
                    if(commentId.Like > 0)
                    {
                        commentId.Like = 0;
                        _context.CommentReactions.Update(commentId);
                        await _context.SaveChangesAsync();
                    }
                    else if (commentId.Heart >0)
                    {
                        commentId.Heart = 0;
                        _context.CommentReactions.Update(commentId);
                        await _context.SaveChangesAsync();
                    }

                }
                else if (alreadyReactsWithSameComment == true && commentId.Like == 0 || commentId.Heart == 0)
                {
                    // changed the like into heart
                    return BadRequest("You already reset the reaction to the comment.");
                }
                else
                {
                    // Already reacts to the same comment.
                    return BadRequest("React first before remove.");
                }
            }
            else
            {
                return BadRequest("You`re not belong to the activity.");
            }
            return Ok("Successfully reset the react to the comments");
        }
    }
}
