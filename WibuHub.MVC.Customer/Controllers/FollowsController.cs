using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;

namespace WibuHub.MVC.Customer.Controllers
{
    [Authorize]
    public class FollowsController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly UserManager<StoryUser> _userManager;

        public FollowsController(StoryDbContext context, UserManager<StoryUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(Guid storyId, string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized();
            }

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == storyId);
            if (story == null)
            {
                return NotFound();
            }

            var existingFollow = await _context.Follows.FindAsync(userGuid, storyId);
            if (existingFollow == null)
            {
                _context.Follows.Add(new Follow { UserId = userGuid, StoryId = storyId });
                story.FollowCount += 1;
            }
            else
            {
                _context.Follows.Remove(existingFollow);
                if (story.FollowCount > 0)
                {
                    story.FollowCount -= 1;
                }
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "Stories", new { id = storyId });
        }
    }
}
