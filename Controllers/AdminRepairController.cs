using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ComBag.Data;
using ComBag.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace ComBag.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminRepairController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminRepairController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: AdminRepair/Services
        public async Task<IActionResult> Services()
        {
            var services = await _context.RepairServices
                .OrderBy(rs => rs.SortOrder)
                .ThenBy(rs => rs.Name)
                .ToListAsync();

            return View(services);
        }

        // GET: AdminRepair/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminRepair/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RepairService repairService, IFormFile imageFile)
        {
            Console.WriteLine("=== CREATE POST CALLED ===");
            Console.WriteLine($"Model is null: {repairService == null}");
            
            // Remove StartingPrice validation since it's removed from model
            ModelState.Remove("StartingPrice");
            ModelState.Remove("EstimatedTimeHours");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"❌ ModelState is INVALID. Error count: {ModelState.ErrorCount}");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"   Field '{state.Key}': {error.ErrorMessage}");
                    }
                }
                return View(repairService);
            }
            
            Console.WriteLine("✅ ModelState is VALID");
            Console.WriteLine($"Name: {repairService.Name}");
            Console.WriteLine($"PriceRange: {repairService.PriceRange}");
            Console.WriteLine($"Duration: {repairService.Duration}");
            
            try
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageUrl = await UploadImage(imageFile);
                    repairService.ImageUrl = imageUrl;
                }

                repairService.CreatedAt = DateTime.UtcNow;
                repairService.UpdatedAt = DateTime.UtcNow;
                
                _context.Add(repairService);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ Service created with ID: {repairService.Id}");
                TempData["Success"] = "Repair service created successfully!";
                return RedirectToAction(nameof(Services));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                ModelState.AddModelError("", "Error creating service: " + ex.Message);
                return View(repairService);
            }
        }

        // GET: AdminRepair/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var repairService = await _context.RepairServices.FindAsync(id);
            if (repairService == null)
            {
                return NotFound();
            }
            
            return View(repairService);
        }

        // POST: AdminRepair/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RepairService repairService, IFormFile imageFile)
        {
            if (id != repairService.Id)
            {
                return NotFound();
            }

            // Remove StartingPrice validation since it's removed from model
            ModelState.Remove("StartingPrice");
            ModelState.Remove("EstimatedTimeHours");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload if new image provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await UploadImage(imageFile);
                        repairService.ImageUrl = imageUrl;
                    }

                    repairService.UpdatedAt = DateTime.UtcNow;
                    _context.Update(repairService);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Service updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(repairService.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Services));
            }
            
            return View(repairService);
        }

        // GET: AdminRepair/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var repairService = await _context.RepairServices
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (repairService == null)
            {
                return NotFound();
            }

            return View(repairService);
        }

        // POST: AdminRepair/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var repairService = await _context.RepairServices.FindAsync(id);
            if (repairService != null)
            {
                // Optional: Delete the associated image file
                if (!string.IsNullOrEmpty(repairService.ImageUrl))
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, repairService.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.RepairServices.Remove(repairService);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service deleted successfully!";
            }

            return RedirectToAction(nameof(Services));
        }

        // GET: AdminRepair/Inquiries
        public async Task<IActionResult> Inquiries(string status = null, string search = null)
        {
            var query = _context.ServiceInquiries
                .Include(si => si.User)
                .Include(si => si.RepairService)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(si => si.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(si => 
                    si.FullName.Contains(search) || 
                    si.Email.Contains(search) ||
                    si.Description.Contains(search));
            }

            var inquiries = await query
                .OrderByDescending(si => si.CreatedAt)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.StatusOptions = new List<string> { "New", "In Progress", "Quoted", "Completed", "Cancelled" };

            return View(inquiries);
        }

        // GET: AdminRepair/InquiryDetails/5
        public async Task<IActionResult> InquiryDetails(int id)
        {
            var inquiry = await _context.ServiceInquiries
                .Include(si => si.User)
                .Include(si => si.RepairService)
                .FirstOrDefaultAsync(si => si.Id == id);

            if (inquiry == null)
            {
                return NotFound();
            }

            return View(inquiry);
        }

        // POST: AdminRepair/UpdateInquiryStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInquiryStatus(int id, string status, string adminNotes, decimal? quotedPrice, DateTime? estimatedCompletionDate)
        {
            var inquiry = await _context.ServiceInquiries.FindAsync(id);
            if (inquiry == null)
            {
                return NotFound();
            }

            inquiry.Status = status;
            inquiry.AdminNotes = adminNotes;
            inquiry.QuotedPrice = quotedPrice;
            inquiry.EstimatedCompletionDate = estimatedCompletionDate;
            inquiry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Inquiry #{id} status updated to {status}";
            return RedirectToAction(nameof(InquiryDetails), new { id });
        }

        private bool ServiceExists(int id)
        {
            return _context.RepairServices.Any(e => e.Id == id);
        }

        // Helper method for image upload
        private async Task<string> UploadImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid file type. Only JPG, PNG, GIF, and WebP are allowed.");

            // Validate file size (max 5MB)
            if (imageFile.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("File size exceeds 5MB limit.");

            // Create unique filename
            var fileName = Guid.NewGuid().ToString() + extension;
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "services");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/uploads/services/{fileName}";
        }
    }
}