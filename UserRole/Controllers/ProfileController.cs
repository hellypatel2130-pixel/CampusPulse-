using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRole.Data;
using UserRole.Models;

namespace UserRole.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<Users> userManager;
        private readonly AppDbContext context;
        private readonly IWebHostEnvironment environment;

        public ProfileController(
            UserManager<Users> userManager,
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            this.userManager = userManager;
            this.context = context;
            this.environment = environment;
        }

        // =========================
        // MY PROFILE
        // =========================

        public async Task<IActionResult> Index()
        {
            var currentUser =
                await userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            ViewBag.Posts =
    await context.Posts
    .CountAsync(x =>
        x.UserId == currentUser.Id &&
        !x.IsHidden);

            ViewBag.Followers =
                await context.Follows
                .CountAsync(x =>
                    x.FollowingId == currentUser.Id &&
                    x.IsAccepted);

            ViewBag.Following =
                await context.Follows
                .CountAsync(x =>
                    x.FollowerId == currentUser.Id &&
                    x.IsAccepted);

            return View(currentUser);
        }

        // =========================
        // VIEW USER PROFILE
        // =========================

        public async Task<IActionResult> UserProfile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var profile =
                await userManager.Users
                .FirstOrDefaultAsync(x => x.Id == id);

            if (profile == null)
            {
                return NotFound();
            }

            ViewBag.Posts =
    await context.Posts
    .CountAsync(x =>
        x.UserId == id &&
        !x.IsHidden);


            ViewBag.Followers =
                await context.Follows
                .CountAsync(x =>
                    x.FollowingId == id &&
                    x.IsAccepted);

            ViewBag.Following =
                await context.Follows
                .CountAsync(x =>
                    x.FollowerId == id &&
                    x.IsAccepted);

            var currentUser =
                await userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                ViewBag.IsFollowing =
     await context.Follows.AnyAsync(x =>
         x.FollowerId == currentUser.Id &&
         x.FollowingId == id &&
         x.IsAccepted);
               
                ViewBag.IsRequestPending =
    await context.Follows.AnyAsync(x =>
        x.FollowerId == currentUser.Id &&
        x.FollowingId == id &&
        !x.IsAccepted);
            }

            return View(profile);

        }

        // =========================
        // SEARCH USERS
        // =========================
        public async Task<IActionResult> Search(string search)
        {
            var users = await userManager.Users
                .Where(x =>
                    string.IsNullOrEmpty(search) ||
                    x.FullName.Contains(search) ||
                    x.Email.Contains(search) ||
                    (x.Department != null && x.Department.Contains(search)) ||
                    (x.EnrollmentNo != null && x.EnrollmentNo.Contains(search)))
                .ToListAsync();

            foreach (var user in users)
            {
                user.Role = (await userManager.GetRolesAsync(user)).FirstOrDefault();
            }

            ViewBag.Search = search;

            return View(users);
        }

        // =========================
        // EDIT PROFILE PAGE
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user =
                await userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            return View(user);
        }

        // =========================
        // UPDATE PROFILE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Users model,
            IFormFile? ProfileImage)
        {
            var user =
                await userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            user.FullName = model.FullName;
            user.Bio = model.Bio;

            // NEW FIELDS
            user.EnrollmentNo = model.EnrollmentNo;
            user.Department = model.Department;

            // =========================
            // PROFILE IMAGE UPLOAD
            // =========================

            if (ProfileImage != null &&
                ProfileImage.Length > 0)
            {
                string uploadsFolder =
                    Path.Combine(
                        environment.WebRootPath,
                        "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(
                        uploadsFolder);
                }

                string fileName =
                    Guid.NewGuid().ToString() +
                    Path.GetExtension(
                        ProfileImage.FileName);

                string filePath =
                    Path.Combine(
                        uploadsFolder,
                        fileName);

                using (var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(
                        stream);
                }

                user.ProfilePicture =
                    "/uploads/" + fileName;
            }

            var result =
                await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(model);
            }

            TempData["Success"] =
                "Profile Updated Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}