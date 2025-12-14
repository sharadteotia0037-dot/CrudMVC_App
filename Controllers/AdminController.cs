using CrudMVC_App.DAL;
using CrudMVC_App.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudMVC_App.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; 

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Admin/Users
        public IActionResult Users(int page = 1)
        {
            int pageSize = 5;

            var users = _userManager.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;

            return View(users);
        }

        // GET: Admin/DeleteUser/id
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Admin/DeleteUser/id
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            // ❌ Prevent self delete
            if (id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                
                user.IsDeleted = true;
                user.DeletedOn = DateTime.Now;
                await _userManager.UpdateAsync(user);
                //await _userManager.DeleteAsync(user);

                _context.AuditLogs.Add(new AuditLog
                {
                    Action = "Delete User",
                    PerformedBy = User.Identity.Name,
                    TargetUser = user.Email,
                    Timestamp = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Users));
        }


        public async Task<IActionResult> EditUserRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.AllRoles = new[] { Roles.Admin, Roles.User };

            return View((user, roles));
        }

        [HttpPost]
        public async Task<IActionResult> EditUserRoles(string userId, string[] roles)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var existingRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, existingRoles);
            await _userManager.AddToRolesAsync(user, roles);

            return RedirectToAction(nameof(Users));
        }

        
        public async Task<IActionResult> RestoreUser(string id)
        {
            var user = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            user.IsDeleted = false;
            user.DeletedOn = null;
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "Restore User",
                PerformedBy = User.Identity.Name,
                TargetUser = user.Email,
                Timestamp = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Users));
        }

        public IActionResult AuditLogs()
        {
            return View(_context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .ToList());
        }

    }
}
