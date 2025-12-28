using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComBag.Data;
using ComBag.Models;
using System.Security.Claims;

namespace ComBag.Controllers
{
    public class RepairController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public RepairController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Repair
        public async Task<IActionResult> Index()
        {
            var services = await _context.RepairServices
                .Where(rs => rs.IsActive)
                .OrderBy(rs => rs.SortOrder)
                .ThenBy(rs => rs.Name)
                .ToListAsync();

            return View(services);
        }

        // GET: /Repair/Inquiry
        public async Task<IActionResult> Inquiry(int? serviceId)
        {
            var model = new ServiceInquiryViewModel();
            
            // Pre-fill user info if logged in
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users.FindAsync(userId);
                
                if (user != null)
                {
                    model.FullName = user.FullName ?? user.UserName;
                    model.Email = user.Email;
                    model.PhoneNumber = user.PhoneNumber;
                }
            }

            // Pre-select service if ID provided
            if (serviceId.HasValue)
            {
                model.RepairServiceId = serviceId.Value;
                var service = await _context.RepairServices.FindAsync(serviceId.Value);
                if (service != null)
                {
                    ViewBag.ServiceName = service.Name;
                }
            }

            ViewBag.Services = await _context.RepairServices
                .Where(rs => rs.IsActive)
                .OrderBy(rs => rs.Name)
                .ToListAsync();

            return View(model);
        }

        // POST: /Repair/Inquiry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inquiry(ServiceInquiryViewModel model, IFormFile damageImage)
        {
            if (ModelState.IsValid)
            {
                var inquiry = new ServiceInquiry
                {
                    UserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    RepairServiceId = model.RepairServiceId,
                    BagType = model.BagType,
                    Description = model.Description,
                    Status = "New",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Handle image upload
                if (damageImage != null && damageImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "repairs");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + damageImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await damageImage.CopyToAsync(stream);
                    }

                    inquiry.DamageImageUrl = "/uploads/repairs/" + uniqueFileName;
                }

                _context.ServiceInquiries.Add(inquiry);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Your repair inquiry has been submitted successfully! We'll contact you soon.";
                return RedirectToAction("Index");
            }

            ViewBag.Services = await _context.RepairServices
                .Where(rs => rs.IsActive)
                .OrderBy(rs => rs.Name)
                .ToListAsync();

            return View(model);
        }
    }

    public class ServiceInquiryViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Select Service")]
        public int? RepairServiceId { get; set; }

        [Display(Name = "Bag Type/Brand (Optional)")]
        public string? BagType { get; set; }

        [Required(ErrorMessage = "Please describe the damage or customization needed")]
        [Display(Name = "Description of Damage/Customization")]
        public string Description { get; set; }

        [Display(Name = "Upload Image of Damage (Optional)")]
        public IFormFile? DamageImage { get; set; }
    }
}