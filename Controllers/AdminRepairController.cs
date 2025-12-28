using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ComBag.Data;
using ComBag.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ComBag.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminRepairController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminRepairController(ApplicationDbContext context)
        {
            _context = context;
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

        // GET: AdminRepair/CreateService
        public IActionResult CreateService()
        {
            return View();
        }

        // POST: AdminRepair/CreateService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(RepairService service)
        {
            if (ModelState.IsValid)
            {
                service.CreatedAt = DateTime.UtcNow;
                service.UpdatedAt = DateTime.UtcNow;

                _context.Add(service);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Service created successfully!";
                return RedirectToAction(nameof(Services));
            }

            return View(service);
        }

        // GET: AdminRepair/EditService/5
        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.RepairServices.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: AdminRepair/EditService/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, RepairService service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    service.UpdatedAt = DateTime.UtcNow;
                    _context.Update(service);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Service updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id))
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

            return View(service);
        }

        // POST: AdminRepair/DeleteService/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.RepairServices.FindAsync(id);
            if (service != null)
            {
                _context.RepairServices.Remove(service);
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

        // POST: AdminRepair/ExportInquiriesToGoogleSheets
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportInquiriesToGoogleSheets(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.ServiceInquiries
                .Include(si => si.RepairService)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(si => si.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(si => si.CreatedAt <= toDate.Value.AddDays(1));
            }

            var inquiries = await query
                .OrderByDescending(si => si.CreatedAt)
                .ToListAsync();

            // For now, export as CSV (we'll implement Google Sheets API later)
            var csv = GenerateInquiriesCSV(inquiries);
            
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"repair-inquiries-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        }

        private bool ServiceExists(int id)
        {
            return _context.RepairServices.Any(e => e.Id == id);
        }

        private string GenerateInquiriesCSV(List<ServiceInquiry> inquiries)
        {
            var csv = "ID,Date,Full Name,Email,Phone,Service,Status,Description,Quoted Price,Admin Notes\n";
            
            foreach (var inquiry in inquiries)
            {
                csv += $"\"{inquiry.Id}\",";
                csv += $"\"{inquiry.CreatedAt:yyyy-MM-dd HH:mm}\",";
                csv += $"\"{EscapeCsvField(inquiry.FullName)}\",";
                csv += $"\"{EscapeCsvField(inquiry.Email)}\",";
                csv += $"\"{EscapeCsvField(inquiry.PhoneNumber)}\",";
                csv += $"\"{EscapeCsvField(inquiry.RepairService?.Name)}\",";
                csv += $"\"{EscapeCsvField(inquiry.Status)}\",";
                csv += $"\"{EscapeCsvField(inquiry.Description)}\",";
                csv += $"\"{inquiry.QuotedPrice?.ToString("N2")}\",";
                csv += $"\"{EscapeCsvField(inquiry.AdminNotes)}\"\n";
            }

            return csv;
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";
            
            // Escape quotes by doubling them
            return field.Replace("\"", "\"\"");
        }
    }
}