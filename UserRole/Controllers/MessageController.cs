using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UserRole.Data;
using UserRole.Hubs;
using UserRole.Models;


namespace UserRole.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly AppDbContext context;
        private readonly UserManager<Users> userManager;
        private readonly IHubContext<ChatHub> chatHub;
        private readonly IWebHostEnvironment environment;
        public MessageController(
    AppDbContext context,
    UserManager<Users> userManager,
    IHubContext<ChatHub> chatHub,
    IWebHostEnvironment environment)
        {
            this.context = context;
            this.userManager = userManager;
            this.chatHub = chatHub;
            this.environment = environment;
        }

        // ==========================================
        // INBOX
        // ==========================================

        public async Task<IActionResult> Inbox()
        {
            var currentUser = await userManager.GetUserAsync(User);

            var following = await context.Follows
                .Include(x => x.Following)
                .Where(x =>
                    x.FollowerId == currentUser.Id &&
                    x.IsAccepted)
                .ToListAsync();

            return View(following);
        }

        // ==========================================
        // OPEN CHAT
        // ==========================================

        public async Task<IActionResult> Chat(string userId)
        {
            var currentUser = await userManager.GetUserAsync(User);

            var otherUser = await userManager.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (otherUser == null)
            {
                return RedirectToAction(nameof(Inbox));
            }

            var conversation = await context.Conversations
                .FirstOrDefaultAsync(x =>

                    (x.User1Id == currentUser.Id &&
                     x.User2Id == userId)

                    ||

                    (x.User1Id == userId &&
                     x.User2Id == currentUser.Id));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    User1Id = currentUser.Id,
                    User2Id = userId
                };

                context.Conversations.Add(conversation);

                await context.SaveChangesAsync();
            }

            var messages = await context.Messages
    .AsNoTracking()
    .Include(x => x.Sender)

    .Include(x => x.SharedPost)
        .ThenInclude(p => p.Users)

    .Include(x => x.SharedPost)
        .ThenInclude(p => p.Likes)

    .Include(x => x.SharedPost)
        .ThenInclude(p => p.Comments)

    .Where(x => x.ConversationId == conversation.Id)
    .OrderBy(x => x.SentAt)
    .ToListAsync();
            ViewBag.OtherUser = otherUser;
            ViewBag.ConversationId = conversation.Id;
            ViewBag.CurrentUser = currentUser;

            return View(messages);
        }

        // ==========================================
        // SEND MESSAGE (AJAX + SIGNALR)
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(
    int conversationId,
    string? text,
    IFormFile? file)
        {
            try
            {
                // pura existing code
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.ToString()
                });
            }
            var currentUser = await userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Json(new
                {
                    success = false
                });
            }

            if (string.IsNullOrWhiteSpace(text) && file == null)
            {
                return Json(new
                {
                    success = false
                });
            }
            var conversation = await context.Conversations
                .FirstOrDefaultAsync(x => x.Id == conversationId);

            if (conversation == null)
            {
                return Json(new
                {
                    success = false
                });
            }
            string? savedPath = null;
            string? originalFileName = null;
            string? contentType = null;
            long? fileSize = null;

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    environment.WebRootPath,
                    "uploads",
                    "chat");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var extension = Path.GetExtension(file.FileName);

                var uniqueFileName =
                    Guid.NewGuid().ToString() + extension;

                var physicalPath =
                    Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(
                    physicalPath,
                    FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                savedPath = "/uploads/chat/" + uniqueFileName;

                originalFileName = file.FileName;

                contentType = file.ContentType;

                fileSize = file.Length;
            }
            Message message = new Message
            {
                ConversationId = conversationId,
                SenderId = currentUser.Id,
                Text = string.IsNullOrWhiteSpace(text) ? "" : text.Trim(),

                FileName = originalFileName,
                FilePath = savedPath,
                FileType = contentType,
                FileSize = fileSize,

                SentAt = DateTime.Now
            };

            context.Messages.Add(message);
            
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
          

            string receiverId =
                conversation.User1Id == currentUser.Id
                    ? conversation.User2Id
                    : conversation.User1Id;

            // ==========================================
            // Notification
            // ==========================================

            Notification notification = new Notification
            {
                UserId = receiverId,
                Title = "New Message",
                Message = $"{currentUser.FullName} sent you a message",
                Url = "/Message/Chat?userId=" + currentUser.Id,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            context.Notifications.Add(notification);

            await context.SaveChangesAsync();

            // ==========================================
            // SignalR Broadcast
            // ==========================================

            await chatHub.Clients
    .Group(conversationId.ToString())
    .SendAsync(
        "ReceiveMessage",
        currentUser.Id,
        currentUser.FullName,
        message.Text,
        message.FileName,
        message.FilePath,
        message.FileType,
        message.FileSize,
        message.SentAt.ToString("dd MMM yyyy • hh:mm tt")
    );
            return Json(new
            {
                success = true,

                senderId = currentUser.Id,
                senderName = currentUser.FullName,

                text = message.Text,

                fileName = message.FileName,
                filePath = message.FilePath,
                fileType = message.FileType,
                fileSize = message.FileSize,

                time = message.SentAt.ToString("dd MMM yyyy • hh:mm tt")
            });
        }
    }
}
