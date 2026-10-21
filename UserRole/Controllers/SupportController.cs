using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRole.Data;
using UserRole.Models;


namespace UserRole.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public SupportController(
            AppDbContext context,
            UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================
        // USER SUPPORT FORM
        // ==========================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new SupportTicket
            {
                Name = user?.FullName,
                Email = user?.Email
            };

            return View(model);
        }
        // ==========================
        // ADMIN - ALL TICKETS
        // ==========================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminTickets()
        {
            var tickets = await _context.SupportTickets
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }
        // ==========================
        // ADMIN - TICKET DETAILS
        // ==========================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.SupportTickets
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        // ==========================
        // UPDATE STATUS
        // ==========================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ticket == null)
                return NotFound();

            ticket.Status = status;

            if (status == "Resolved")
            {
                ticket.ResolvedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();


            // ======================================
            // SEND NOTIFICATION TO TICKET OWNER
            // ======================================

            string title;
            string message;
            string icon;
            string color;

            if (status == "Resolved")
            {
                title = "Support Ticket Resolved";

                message =
                    $"Your support ticket ({ticket.TicketNumber}) has been resolved successfully.";

                icon = "fa-solid fa-circle-check";

                color = "success";
            }
            else
            {
                title = "Support Ticket Updated";

                message =
                    $"Your support ticket ({ticket.TicketNumber}) is now {status}.";

                icon = "fa-solid fa-headset";

                color = "warning";
            }

            _context.Notifications.Add(new Notification
            {
                UserId = ticket.UserId,

                Title = title,

                Message = message,

                Url = $"/Support/MyTicketDetails/{ticket.Id}",

                NotificationType = "Support",

                Icon = icon,

                Color = color,

                IsRead = false,

                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ticket status updated successfully.";

            return RedirectToAction(nameof(Details), new { id });
        }
        // ==========================
        // USER - MY TICKETS
        // ==========================

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyTickets()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var tickets = await _context.SupportTickets
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }
        // ==========================
        // USER - MY TICKET DETAILS
        // ==========================

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyTicketDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == user.Id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // ==========================
        // SUBMIT TICKET
        // ==========================
        // ==========================
        // SUBMIT TICKET
        // ==========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupportTicket model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Re-fill readonly fields
            model.Name = user.FullName;
            model.Email = user.Email;
            model.UserId = user.Id;

            if (!ModelState.IsValid)
            {
                // Uncomment this while debugging if needed
                /*
                foreach (var error in ModelState)
                {
                    foreach (var e in error.Value.Errors)
                    {
                        Console.WriteLine($"{error.Key} : {e.ErrorMessage}");
                    }
                }
                */

                return View(model);
            }

            model.TicketNumber = "CP-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

            model.Status = "Pending";
            model.CreatedAt = DateTime.Now;
            model.ResolvedAt = null;

            _context.SupportTickets.Add(model);

            await _context.SaveChangesAsync();


            // ==========================
            // SEND NOTIFICATION TO ADMIN
            // ==========================

            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            foreach (var admin in admins)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = admin.Id,

                    Title = "New Support Ticket",

                    Message = $"{user.FullName} submitted a support request.",

                    Url = "/Support/AdminTickets",

                    NotificationType = "Support",

                    Icon = "fa-solid fa-headset",

                    Color = "warning",

                    IsRead = false,

                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Support ticket submitted successfully.";

            return RedirectToAction(nameof(Create));
        }
    }
}