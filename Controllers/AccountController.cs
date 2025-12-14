using CrudMVC_App.BLL.Services;
using CrudMVC_App.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrudMVC_App.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailService emailservice)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailservice;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid) return View(model);



            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            


            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // ✅ Assign User role
                await _userManager.AddToRoleAsync(user, Roles.User);
                // ✅ Auto login
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Products");

                //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                //var confirmationLink = Url.Action(
                //    "ConfirmEmail",
                //    "Account",
                //    new { userId = user.Id, token = token },
                //    Request.Scheme);

                //await _emailService.SendAsync(
                //    user.Email,
                //    "Confirm your email",
                //    $"Click <a href='{confirmationLink}'>here</a> to confirm");

                //return View("RegistrationSuccess");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);

            return result.Succeeded
                ? View()
                : BadRequest();
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction("Verify2FA");
            }
            if (result.Succeeded)
                return RedirectToAction("Index", "Products");

            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Verify2FA(string code)
        {
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                code,
                isPersistent: false,
                rememberClient: false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Products");

            ModelState.AddModelError("", "Invalid authentication code");
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
