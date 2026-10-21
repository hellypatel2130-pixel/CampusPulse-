using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserRole.Data;
using UserRole.Hubs;
using UserRole.Models;
using UserRoles.Models;
using UserRoles.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// MVC
// ==========================================

builder.Services.AddControllersWithViews();

// ==========================================
// Database
// ==========================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

// ==========================================
// Identity
// ==========================================

builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ==========================================
// Authentication Cookie Settings
// ==========================================

builder.Services.ConfigureApplicationCookie(options =>
{
    // Login page
    options.LoginPath = "/Account/Login";

    // Access denied page
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Cookie expires after 30 days
    options.ExpireTimeSpan = TimeSpan.FromDays(30);

    // Extend cookie if user is active
    options.SlidingExpiration = true;

    // Security
    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;

    options.Cookie.Name = "CampusPulseAuth";
});

// ==========================================
// Email Service
// ==========================================

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddTransient<IEmailService, EmailService>();

// ==========================================
// SignalR
// ==========================================

builder.Services.AddSignalR();

// ==========================================
// Build App
// ==========================================

var app = builder.Build();

// ==========================================
// Error Handling
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ==========================================
// Middleware
// ==========================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// ==========================================
// MVC Route
// ==========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==========================================
// SignalR Hubs
// ==========================================

app.MapHub<NotificationHub>("/notificationHub");

app.MapHub<ChatHub>("/chatHub");

// ==========================================
// Run
// ==========================================

app.Run();
