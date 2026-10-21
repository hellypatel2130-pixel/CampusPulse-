using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UserRole.Data;
using UserRole.Models;

namespace UserRole.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly AppDbContext context;
        private readonly UserManager<Users> userManager;

        public HomeController(
            ILogger<HomeController> logger,
            AppDbContext context,
            UserManager<Users> userManager)
        {
            this.logger = logger;
            this.context = context;
            this.userManager = userManager;
        }

        // =========================
        // HOME PAGE
        // =========================


        public IActionResult Index()
        {
            ViewBag.TotalUsers = context.Users.Count();

            ViewBag.TotalPosts = context.Posts.Count(x => !x.IsHidden);

            ViewBag.RecentPosts = new List<Post>();

            return View();
        }

        // =========================
        // PRIVACY
        // =========================

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        // =========================
        // ADMIN REDIRECT
        // =========================

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return RedirectToAction(nameof(AdminDashboard));
        }

        // =========================
        // USER REDIRECT
        // =========================

        [Authorize(Roles = "User")]
        public new IActionResult User()
        {
            return RedirectToAction(nameof(UserDashboard));
        }

        // =========================
        // ADMIN DASHBOARD
        // =========================
        

        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            ViewBag.TotalUsers = context.Users.Count();

            ViewBag.TotalPosts = context.Posts.Count(x => !x.IsHidden);

            ViewBag.TotalLikes = context.Likes.Count(l => !l.Post.IsHidden);

            ViewBag.TotalFiles = context.Posts.Count(x => !x.IsHidden);

            ViewBag.TotalLikes = context.Likes.Count(l =>
    context.Posts.Any(p => p.Id == l.PostId && !p.IsHidden));
            ViewBag.TotalImages = context.Posts.Count(x =>
    !x.IsHidden &&
    x.FileType.StartsWith("image"));

            ViewBag.TotalVideos = context.Posts.Count(x =>
    !x.IsHidden &&
    x.FileType.StartsWith("video"));

            ViewBag.TotalDocuments = context.Posts.Count(x =>
    !x.IsHidden &&
    !x.FileType.StartsWith("image") &&
    !x.FileType.StartsWith("video"));

            ViewBag.RecentPosts = context.Posts
    .Where(x => !x.IsHidden)
    .OrderByDescending(x => x.CreatedAt)
    .Take(5)
    .ToList();
            
           
            ViewBag.TotalSupportTickets = context.SupportTickets.Count();

            ViewBag.PendingSupportTickets = context.SupportTickets
                .Count(x => x.Status == "Pending");

            ViewBag.ResolvedSupportTickets = context.SupportTickets
                .Count(x => x.Status == "Resolved");

            ViewBag.HighPriorityTickets = context.SupportTickets
                .Count(x => x.Priority == "High");
            return View();
        }

        // =========================
        // USER DASHBOARD
        // =========================

        [Authorize]
        public async Task<IActionResult> UserDashboard()
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            ViewBag.Posts =
                context.Posts.Count(x =>
                    x.UserId == user.Id);

            ViewBag.Followers =
                context.Follows.Count(x =>
                    x.FollowingId == user.Id &&
                    x.IsAccepted);

            ViewBag.Following =
                context.Follows.Count(x =>
                    x.FollowerId == user.Id &&
                    x.IsAccepted);

            int profileScore = 0;

            if (!string.IsNullOrEmpty(user.FullName))
                profileScore += 20;

            if (!string.IsNullOrEmpty(user.Bio))
                profileScore += 20;

            if (!string.IsNullOrEmpty(user.ProfilePicture))
                profileScore += 20;

            if (!string.IsNullOrEmpty(user.Department))
                profileScore += 20;

            if (!string.IsNullOrEmpty(user.EnrollmentNo))
                profileScore += 20;

            ViewBag.ProfileCompletion = profileScore;

            return View(user);
        }

        // =========================
        // ERROR PAGE
        // =========================

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
                });
        }
    }
}