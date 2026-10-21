using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRole.Data;
using UserRole.Models;

namespace UserRole.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public NotificationController(
            AppDbContext context,
            UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===============================
        // NOTIFICATION LIST
        // ===============================

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(notifications);
        }

        // ===============================
        // UNREAD COUNT (AJAX)
        // ===============================

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Json(0);

            int count = await _context.Notifications
                .CountAsync(x =>
                    x.UserId == user.Id &&
                    !x.IsRead);

            return Json(count);
        }

        // ===============================
        // MARK SINGLE AS READ
        // ===============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == user.Id);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // ===============================
        // MARK ALL AS READ
        // ===============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var notifications = await _context.Notifications
                .Where(x =>
                    x.UserId == user.Id &&
                    !x.IsRead)
                .ToListAsync();

            foreach (var item in notifications)
            {
                item.IsRead = true;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "All notifications marked as read.";

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // DELETE SINGLE NOTIFICATION
        // ===============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == user.Id);

            if (notification == null)
                return NotFound();

            _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Notification deleted.";

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // DELETE ALL NOTIFICATIONS
        // ===============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var notifications = await _context.Notifications
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);

            await _context.SaveChangesAsync();

            TempData["Success"] = "All notifications deleted.";

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // REDIRECT FROM NOTIFICATION
        // ===============================

        public async Task<IActionResult> Open(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == user.Id);

            if (notification == null)
                return RedirectToAction(nameof(Index));

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(notification.Url))
            {
                return LocalRedirect(notification.Url);
            }

            return RedirectToAction(nameof(Index));
        
    }
    }
}