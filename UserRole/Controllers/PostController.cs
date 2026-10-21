using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRole.Data;
using UserRole.Models;
using UserRole.ViewModel;
using Microsoft.AspNetCore.SignalR;
using UserRole.Hubs;

namespace UserRole.Controllers
{
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<Users> _userManager;
        private readonly IHubContext<ChatHub> _chatHub;

        public PostController(
    AppDbContext context,
    IWebHostEnvironment environment,
    UserManager<Users> userManager,
    IHubContext<ChatHub> chatHub)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
            _chatHub = chatHub;
        }

        //==========================================================
        // CREATE PAGE
        //==========================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePostViewModel());
        }

        //==========================================================
        // CREATE POST
        //==========================================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.MediaFile == null || model.MediaFile.Length == 0)
            {
                ModelState.AddModelError("MediaFile", "Please select a file.");
                return View(model);
            }

            const long maxSize = 100 * 1024 * 1024;

            if (model.MediaFile.Length > maxSize)
            {
                ModelState.AddModelError("MediaFile", "Maximum allowed file size is 100 MB.");
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            string extension = Path.GetExtension(model.MediaFile.FileName).ToLower();


            // Allowed file extensions
            var allowedExtensions = new[]
            {
    ".jpg", ".jpeg", ".png", ".gif",
    ".mp4", ".pdf", ".doc", ".docx",
    ".xls", ".xlsx", ".ppt", ".pptx", ".zip"
};

            // Validate extension
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("MediaFile", "Unsupported file type.");
                return View(model);
            }

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid() + extension;

            string physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await model.MediaFile.CopyToAsync(stream);
            }

            Post post = new Post
            {
                Caption = model.Caption?.Trim() ?? "",
                FileName = model.MediaFile.FileName,
                FileType = extension,
                FilePath = "/uploads/" + uniqueFileName,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.Posts.Add(post);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Post published successfully.";

            return RedirectToAction(nameof(Feed));
        }

        //==========================================================
        // AI CAPTION
        //==========================================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> GenerateCaption(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new
                {
                    success = false,
                    caption = "Please select a file first."
                });
            }

            string extension = Path.GetExtension(file.FileName).ToLower();

            string[] captions;

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                case ".webp":

                    captions = new[]
                    {
                        "Capturing unforgettable campus memories 📸",
                        "Another beautiful day on campus ❤️",
                        "Moments that matter ✨",
                        "Sharing today's highlights 🎉",
                        "Campus life at its best 🌸",
                        "Memories worth keeping 📷"
                    };
                    break;

                case ".mp4":
                case ".mov":
                case ".avi":
                case ".mkv":
                case ".webm":

                    captions = new[]
                    {
                        "Watch the latest campus moments 🎥",
                        "Video update from our college 🎬",
                        "Don't miss this amazing clip ▶️",
                        "Campus highlights are here 🍿"
                    };
                    break;

                case ".pdf":

                    captions = new[]
                    {
                        "Official PDF uploaded 📄",
                        "Please read the attached document.",
                        "Important notice for students.",
                        "Reference material uploaded."
                    };
                    break;

                case ".doc":
                case ".docx":

                    captions = new[]
                    {
                        "Word document uploaded 📝",
                        "Assignment attached.",
                        "Please review this document."
                    };
                    break;

                case ".xls":
                case ".xlsx":

                    captions = new[]
                    {
                        "Excel report uploaded 📊",
                        "Spreadsheet shared.",
                        "Data file is ready."
                    };
                    break;

                case ".ppt":
                case ".pptx":

                    captions = new[]
                    {
                        "Presentation uploaded 🎤",
                        "Slides are now available.",
                        "Check out the presentation."
                    };
                    break;

                case ".zip":
                case ".rar":
                case ".7z":

                    captions = new[]
                    {
                        "Project archive uploaded 📦",
                        "Resources packed together.",
                        "Download the complete project."
                    };
                    break;

                case ".cs":
                case ".java":
                case ".cpp":
                case ".c":
                case ".py":
                case ".html":
                case ".css":
                case ".js":
                case ".sql":

                    captions = new[]
                    {
                        "Programming project uploaded 💻",
                        "Source code shared successfully.",
                        "Latest development update."
                    };
                    break;

                default:

                    captions = new[]
                    {
                        "New file uploaded successfully 📁",
                        "Please check the attachment.",
                        "Shared with the community."
                    };
                    break;
            }

            await Task.Delay(700);

            Random random = new();

            return Json(new
            {
                success = true,
                caption = captions[random.Next(captions.Length)]
            });
        }
        //==========================================================
        // FEED
        //==========================================================

        [Authorize]
        public async Task<IActionResult> Feed()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            ViewBag.CurrentUser = currentUser;

            var posts = await _context.Posts
                .Include(p => p.Users)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Users)
                .Where(p => !p.IsHidden)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        //==========================================================
        // LIKE
        //==========================================================
     

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int postId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Json(new { success = false });

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return Json(new { success = false });

            bool alreadyLiked = await _context.Likes
                .AnyAsync(x => x.PostId == postId &&
                               x.UserId == user.Id);

            if (!alreadyLiked)
            {
                _context.Likes.Add(new Like
                {
                    PostId = postId,
                    UserId = user.Id
                });

                if (post.UserId != user.Id)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = post.UserId,
                        Title = "New Like",
                        Message = $"{user.FullName} liked your post.",
                        Url = "/Post/Feed",
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });
                }

                await _context.SaveChangesAsync();
            }

            int totalLikes = await _context.Likes
                .CountAsync(x => x.PostId == postId);

            return Json(new
            {
                success = true,
                likes = totalLikes
            });
        }

        //==========================================================
        // COMMENT
        //==========================================================

        //==========================================================
        // COMMENT (AJAX)
        //==========================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(int postId, string message)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Json(new { success = false });

            if (string.IsNullOrWhiteSpace(message))
                return Json(new { success = false });

            var post = await _context.Posts
                .FirstOrDefaultAsync(x => x.Id == postId);

            if (post == null)
                return Json(new { success = false });

            Comment comment = new Comment
            {
                PostId = postId,
                UserId = user.Id,
                Message = message.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);

            if (post.UserId != user.Id)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = post.UserId,
                    Title = "New Comment",
                    Message = $"{user.FullName} commented on your post.",
                    Url = "/Post/Feed",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }

            await _context.SaveChangesAsync();

            int totalComments = await _context.Comments
                .CountAsync(x => x.PostId == postId);

            return Json(new
            {
                success = true,
                comments = totalComments,
                user = user.FullName,
                text = comment.Message,
                date = comment.CreatedAt.ToString("dd MMM yyyy • hh:mm tt"),
                avatar = string.IsNullOrWhiteSpace(user.ProfilePicture)
                    ? "/images/default-user.png"
                    : user.ProfilePicture
            });
        }
        //==========================================================
        // DELETE POST
        //==========================================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return RedirectToAction(nameof(Feed));

            // Delete physical file
            if (!string.IsNullOrWhiteSpace(post.FilePath))
            {
                string relativePath = post.FilePath.TrimStart('/');

                string physicalPath = Path.Combine(
                    _environment.WebRootPath,
                    relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            if (post.Likes.Any())
                _context.Likes.RemoveRange(post.Likes);

            if (post.Comments.Any())
                _context.Comments.RemoveRange(post.Comments);

            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Post deleted successfully.";

            return RedirectToAction(nameof(Feed));
        }

        //==========================================================
        // DOWNLOAD FILE
        //==========================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
                return NotFound();

            string relativePath = post.FilePath.TrimStart('/');

            string physicalPath = Path.Combine(
                _environment.WebRootPath,
                relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!System.IO.File.Exists(physicalPath))
                return NotFound();

            string contentType = "application/octet-stream";

            return PhysicalFile(
                physicalPath,
                contentType,
                post.FileName);
        }

        //==========================================================
        // OPEN FILE
        //==========================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Open(int id)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
                return NotFound();

            return Redirect(post.FilePath);
        }
        [HttpGet]
        public async Task<IActionResult> SharePost(int postId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var users = await _context.Follows
                .Include(x => x.Following)
                .Where(x =>
                    x.FollowerId == currentUser.Id &&
                    x.IsAccepted)
                .Select(x => x.Following)
                .ToListAsync();

            ViewBag.PostId = postId;

            return PartialView("_SharePostModal", users);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendSharedPost(string receiverId, int postId)
        {
            var sender = await _userManager.GetUserAsync(User);

            if (sender == null)
                return Json(new { success = false });

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(x =>
                    (x.User1Id == sender.Id && x.User2Id == receiverId) ||
                    (x.User1Id == receiverId && x.User2Id == sender.Id));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    User1Id = sender.Id,
                    User2Id = receiverId,
                    CreatedAt = DateTime.Now
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            var post = await _context.Posts
                .Include(x => x.Users)
                .FirstOrDefaultAsync(x => x.Id == postId);

            if (post == null)
                return Json(new { success = false });

            Message message = new Message
            {
                ConversationId = conversation.Id,
                SenderId = sender.Id,
                SharedPostId = post.Id,
                Text = "",
                SentAt = DateTime.Now
            };

            _context.Messages.Add(message);

            await _context.SaveChangesAsync();
            await _chatHub.Clients
    .Group(conversation.Id.ToString())
    .SendAsync(
        "ReceiveSharedPost",

        sender.Id,
        sender.FullName,

        post.Id,
        post.Caption ?? "",

        post.FilePath ?? "",
        post.FileType ?? "",

        post.Users?.FullName ?? "Admin",

        message.SentAt.ToString("dd MMM yyyy • hh:mm tt")
    );

            return Json(new
            {
                success = true
            });
        
    }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(x => x.Users)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
                return NotFound();

            return View(post);
        }
    }
}