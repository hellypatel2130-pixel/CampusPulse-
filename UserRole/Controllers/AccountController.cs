using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserRole.Models;
using UserRole.ViewModel;
using UserRoles.Services;

namespace UserRole.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IEmailService emailservice;

        public AccountController(
            SignInManager<Users> signInManager,
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.emailservice = emailService;
        }

        // ================= LOGIN =================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid Login Attempt");
            return View(model);
        }

        // ================= REGISTER =================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Users user = new Users()
            {
                FullName = model.Name,
                Email = model.Email,
                UserName = model.Email,

                EnrollmentNo = model.EnrollmentNo,
                Department = model.Department,

                NormalizedEmail = model.Email.ToUpper(),
                NormalizedUserName = model.Email.ToUpper()
            };

            var result =
                await userManager.CreateAsync(
                    user,
                    model.Password);

            if (result.Succeeded)
            {
                bool roleExists =
                    await roleManager.RoleExistsAsync("User");

                if (!roleExists)
                {
                    await roleManager.CreateAsync(
                        new IdentityRole("User"));
                }

                await userManager.AddToRoleAsync(
                    user,
                    "User");

                await signInManager.SignInAsync(
                    user,
                    false);

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description);
            }

            return View(model);
        }

        // ================= VERIFY EMAIL =================

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(
            VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user =
                await userManager.FindByEmailAsync(
                    model.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "User not found");

                return View(model);
            }

            var resetToken =
                await userManager
                .GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(
                "ChangePassword",
                "Account",
                new
                {
                    email = model.Email,
                    token = resetToken
                },
                Request.Scheme);

            var subject = "Reset Password";

            var body =
                $"Please reset your password by clicking here: <a href='{resetLink}'>Reset Password</a>";

            await emailservice.SendEmailAsync(
                model.Email,
                subject,
                body);

            return RedirectToAction(
                "EmailSent",
                "Account");
        }

        // ================= CHANGE PASSWORD GET =================

        [HttpGet]
        public IActionResult ChangePassword(
            string email,
            string token)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "VerifyEmail",
                    "Account");
            }

            var model =
                new ChangePasswordViewModel
                {
                    Email = email,
                    Token = token
                };

            return View(model);
        }

        // ================= CHANGE PASSWORD POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(
                    "",
                    "Something went wrong");

                return View(model);
            }

            var user =
                await userManager.FindByEmailAsync(
                    model.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "User not found!");

                return View(model);
            }

            var resetResult =
                await userManager.ResetPasswordAsync(
                    user,
                    model.Token,
                    model.NewPassword);

            if (!resetResult.Succeeded)
            {
                foreach (var error in resetResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(model);
            }

            return RedirectToAction(
                "Login",
                "Account");
        }

        // ================= EMAIL SENT =================

        [HttpGet]
        public IActionResult EmailSent()
        {
            return View();
        }

        // ================= LOGOUT =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}