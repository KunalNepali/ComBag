using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ComBag.Data;
using ComBag.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ComBag.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: AdminProducts
        public async Task<IActionResult> Index(string search, int? categoryId)
        {
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => 
                    p.Name.Contains(search) || 
                    p.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
            }

            var products = await productsQuery.ToListAsync();
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            return View(products);
        }

        // GET: AdminProducts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: AdminProducts/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: AdminProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = "/uploads/" + uniqueFileName;
                }
                else
                {
                    product.ImageUrl = "/images/placeholder.jpg";
                }

                product.CreatedAt = DateTime.UtcNow;
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View(product);
        }

        // GET: AdminProducts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View(product);
        }

        // POST: AdminProducts/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Product product, IFormFile imageFile)
{
    // DEBUG: Log what's coming in
    Console.WriteLine($"=== EDIT POST START ===");
    Console.WriteLine($"URL ID: {id}, Product ID: {product?.Id}");
    Console.WriteLine($"Name: '{product?.Name}', Price: {product?.Price}, Stock: {product?.StockQuantity}");
    
    if (id != product?.Id)
    {
        Console.WriteLine($"ID MISMATCH: {id} != {product?.Id}");
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        Console.WriteLine("ModelState is VALID");
        try
        {
            // Get the existing product from database
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                Console.WriteLine($"Product {id} not found in database");
                return NotFound();
            }

            Console.WriteLine($"Found existing product: {existingProduct.Name}");
            
            // Update properties
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.CategoryId = product.CategoryId;

            Console.WriteLine($"Updated: Name='{existingProduct.Name}', Price={existingProduct.Price}");

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                Console.WriteLine($"New image uploaded: {imageFile.FileName}");
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl) && 
                    existingProduct.ImageUrl != "/images/placeholder.jpg")
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, 
                        existingProduct.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                existingProduct.ImageUrl = "/uploads/" + uniqueFileName;
                Console.WriteLine($"Image updated to: {existingProduct.ImageUrl}");
            }

            // Mark as modified and save
            _context.Entry(existingProduct).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            
            Console.WriteLine($"SaveChangesAsync result: {result} rows affected");
            
            if (result > 0)
            {
                TempData["Success"] = "Product updated successfully!";
                Console.WriteLine("Redirecting to Index...");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("No rows affected - update failed");
                TempData["Error"] = "Failed to update product.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION: {ex.Message}");
            Console.WriteLine($"INNER EXCEPTION: {ex.InnerException?.Message}");
            ModelState.AddModelError("", $"Error saving product: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("ModelState is INVALID");
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine($"Validation Error: {error.ErrorMessage}");
        }
    }
    // If we get here, reload categories
    ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
    Console.WriteLine("=== EDIT POST END ===");
    return View(product);
}
       // GET: AdminProducts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: AdminProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Delete associated image file
                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "/images/placeholder.jpg")
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}