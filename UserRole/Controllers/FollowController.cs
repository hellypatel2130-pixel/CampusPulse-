using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRole.Data;
using UserRole.Models;

namespace UserRole.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly AppDbContext context;
        private readonly UserManager<Users> userManager;

        public FollowController(
            AppDbContext context,
            UserManager<Users> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        // =========================
        // SEND FOLLOW REQUEST
        // =========================

        public async Task<IActionResult> Send(string id)
        {
            var currentUser =
                await userManager.GetUserAsync(User);

            if (currentUser == null ||
                string.IsNullOrEmpty(id) ||
                id == currentUser.Id)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            bool alreadyExists =
                await context.Follows.AnyAsync(x =>
                    x.FollowerId == currentUser.Id &&
                    x.FollowingId == id);

            if (!alreadyExists)
            {
                Follow follow = new Follow
                {
                    FollowerId = currentUser.Id,
                    FollowingId = id,
                    IsAccepted = false
                };

                context.Follows.Add(follow);

                // =========================
                // FOLLOW REQUEST NOTIFICATION
                // =========================

                Notification notification =
                    new Notification
                    {
                        UserId = id,
                        Title = "Follow Request",
                        Message =
                            $"{currentUser.FullName} sent you a follow request",
                        Url = "/Follow/Requests",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };

                context.Notifications.Add(notification);

                await context.SaveChangesAsync();
            }

            return RedirectToAction(
                "UserProfile",
                "Profile",
                new { id });
        }

        // =========================
        // FOLLOW REQUESTS PAGE
        // =========================

        public async Task<IActionResult> Requests()
        {
            var currentUser =
                await userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var requests =
                await context.Follows
                    .Include(x => x.Follower)
                    .Where(x =>
                        x.FollowingId == currentUser.Id &&
                        !x.IsAccepted)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();

            return View(requests);
        }

        // =========================
        // ACCEPT REQUEST
        // =========================

        public async Task<IActionResult> Accept(int id)
        {
            var request =
                await context.Follows
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                return RedirectToAction("Requests");
            }

            request.IsAccepted = true;

            var currentUser =
                await userManager.GetUserAsync(User);

            // =========================
            // ACCEPT NOTIFICATION
            // =========================

            Notification notification =
                new Notification
                {
                    UserId = request.FollowerId,
                    Title = "Follow Request Accepted",
                    Message =
                        $"{currentUser?.FullName} accepted your follow request",
                    Url = "/Profile/UserProfile?id=" +
                          request.FollowingId,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

            context.Notifications.Add(notification);

            await context.SaveChangesAsync();

            return RedirectToAction("Requests");
        }

        // =========================
        // REJECT REQUEST
        // =========================

        public async Task<IActionResult> Reject(int id)
        {
            var request =
                await context.Follows
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (request != null)
            {
                context.Follows.Remove(request);

                await context.SaveChangesAsync();
            }

            return RedirectToAction("Requests");
        }
    }
}