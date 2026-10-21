using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UserRole.Data;
using UserRole.Models;

namespace UserRole.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ChatHub(
            AppDbContext context,
            UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //===========================================
        // USER CONNECTED
        //===========================================

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User);

            if (user != null)
            {
                user.IsOnline = true;
                user.ConnectionId = Context.ConnectionId;

                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            await base.OnConnectedAsync();
        }

        //===========================================
        // USER DISCONNECTED
        //===========================================

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userManager.GetUserAsync(Context.User);

            if (user != null)
            {
                user.IsOnline = false;
                user.ConnectionId = null;
                user.LastSeen = DateTime.Now;

                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            await base.OnDisconnectedAsync(exception);
        }

        //===========================================
        // JOIN CHAT
        //===========================================

        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                conversationId);
        }

        //===========================================
        // LEAVE CHAT
        //===========================================

        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                conversationId);
        }
    }
}