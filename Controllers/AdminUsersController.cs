using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ComBag.Models;
using ComBag.Data;

namespace ComBag.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager, 
                                  RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: AdminUsers
        public async Task<IActionResult> Index(string search, string role)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                usersQuery = usersQuery.Where(u => 
                    u.Email.Contains(search) || 
                    u.FullName.Contains(search) ||
                    u.UserName.Contains(search));
            }

            var users = await usersQuery.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    IsLockedOut = await _userManager.IsLockedOutAsync(user)
                });
            }

            // Role filter
            if (!string.IsNullOrEmpty(role))
            {
                userViewModels = userViewModels.Where(u => u.Roles.Contains(role)).ToList();
            }

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            return View(userViewModels);
        }

        // GET: AdminUsers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                IsLockedOut = await _userManager.IsLockedOutAsync(user)
            };

            return View(viewModel);
        }

        // GET: AdminUsers/EditRoles/5
        public async Task<IActionResult> EditRoles(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var model = new EditRolesViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                UserRoles = userRoles.ToList(),
                AllRoles = allRoles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

        // POST: AdminUsers/EditRoles/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(string id, List<string> selectedRoles)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove roles not selected
            var rolesToRemove = currentRoles.Except(selectedRoles);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            
            // Add new roles
            var rolesToAdd = selectedRoles.Except(currentRoles);
            await _userManager.AddToRolesAsync(user, rolesToAdd);

            TempData["Success"] = $"Roles updated for {user.Email}";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: AdminUsers/ToggleLockout/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockout(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                // Unlock the user
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["Success"] = $"User {user.Email} has been unlocked.";
            }
            else
            {
                // Lock the user for 30 days
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(30));
                TempData["Success"] = $"User {user.Email} has been locked for 30 days.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: AdminUsers/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var newPassword = GenerateRandomPassword();
            
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            
            if (result.Succeeded)
            {
                TempData["PasswordReset"] = $"Password for {user.Email} has been reset to: {newPassword}";
                TempData["Success"] = "Password reset successful!";
            }
            else
            {
                TempData["Error"] = "Failed to reset password.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private string GenerateRandomPassword()
        {
            // Generate a random 8-character password
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public List<string> Roles { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsLockedOut { get; set; }
    }

    public class EditRolesViewModel
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public List<string> UserRoles { get; set; }
        public List<string> AllRoles { get; set; }
    }
}