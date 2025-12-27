using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ComBag.Data;
using ComBag.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ComBag.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminBlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminBlog
        public async Task<IActionResult> Index(string search, bool? published, int? categoryId)
        {
            var query = _context.BlogPosts.Include(b => b.BlogCategory).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.Contains(search) || b.Content.Contains(search));
            }

            if (published.HasValue)
            {
                query = query.Where(b => b.IsPublished == published.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.BlogCategoryId == categoryId);
            }

            // Apply ordering at the end
            query = query.OrderByDescending(b => b.PublishedDate);
            
            var posts = await query.ToListAsync();
            
            ViewBag.Categories = new SelectList(await _context.BlogCategories.ToListAsync(), "Id", "Name");
            ViewBag.Search = search;
            ViewBag.Published = published;
            ViewBag.CategoryId = categoryId;

            return View(posts);
        }

        // GET: AdminBlog/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.BlogCategories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: AdminBlog/Create - ✅ KEEP ONLY THIS ONE
       [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(BlogPost blogPost, IFormFile featuredImage)
{
    // Debug output
    Console.WriteLine($"=== CREATE BLOG POST ===");
    Console.WriteLine($"BlogCategoryId from form: {blogPost.BlogCategoryId}");
    Console.WriteLine($"FeaturedImage from form: {featuredImage?.FileName}");
    
    // Check if BlogCategoryId is 0 (empty selection)
    if (blogPost.BlogCategoryId == 0)
    {
        blogPost.BlogCategoryId = null; // Set to null if no category selected
        Console.WriteLine("BlogCategoryId was 0, set to null");
    }
    
    // Manually remove the FeaturedImageUrl validation error if we have a file
    if (featuredImage != null && featuredImage.Length > 0)
    {
        ModelState.Remove("FeaturedImageUrl");
    }
    
    if (ModelState.IsValid)
    {
        try
        {
            // Generate slug if not provided
            if (string.IsNullOrEmpty(blogPost.Slug))
            {
                blogPost.Slug = GenerateSlug(blogPost.Title);
            }

            // Handle image upload
            if (featuredImage != null && featuredImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "blog");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + featuredImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await featuredImage.CopyToAsync(stream);
                }

                blogPost.FeaturedImageUrl = "/uploads/blog/" + uniqueFileName;
            }
            // If no image uploaded, set to null or default
            else
            {
                blogPost.FeaturedImageUrl = null; // or "/images/default-blog.jpg"
            }

            blogPost.PublishedDate = DateTime.UtcNow;
            blogPost.LastUpdated = DateTime.UtcNow;

            _context.Add(blogPost);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"Blog post saved! ID: {blogPost.Id}");
            
            TempData["Success"] = "Blog post created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            ModelState.AddModelError("", $"Error saving blog post: {ex.Message}");
        }
    }
    else
    {
        // Log all validation errors
        Console.WriteLine("ModelState INVALID:");
        foreach (var key in ModelState.Keys)
        {
            var errors = ModelState[key].Errors;
            if (errors.Any())
            {
                Console.WriteLine($"  {key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
            }
        }
    }

    ViewBag.Categories = new SelectList(await _context.BlogCategories.ToListAsync(), "Id", "Name");
    return View(blogPost);
 }
 
        public async Task<IActionResult> Edit(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _context.BlogCategories.ToListAsync(), "Id", "Name");
            return View(blogPost);
        }

        // POST: AdminBlog/Edit/5
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, BlogPost blogPost, IFormFile featuredImage)
{
    Console.WriteLine($"=== EDIT BLOG POST DEBUG ===");
    Console.WriteLine($"ID: {id}, BlogPost.ID: {blogPost.Id}");
    Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
    Console.WriteLine($"Title: {blogPost.Title}");
    Console.WriteLine($"CategoryId: {blogPost.BlogCategoryId}");
    Console.WriteLine($"FeaturedImageUrl (existing): {blogPost.FeaturedImageUrl}");

    if (id != blogPost.Id)
    {
        Console.WriteLine($"ID mismatch: {id} != {blogPost.Id}");
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        Console.WriteLine("ModelState is VALID - Attempting to update...");
        
        try
        {
            // Get the existing blog post from database
            var existingPost = await _context.BlogPosts.FindAsync(id);
            if (existingPost == null)
            {
                Console.WriteLine($"Blog post {id} not found in database");
                return NotFound();
            }

            Console.WriteLine($"Found existing post: {existingPost.Title}");

            // Handle image upload if new file is provided
            if (featuredImage != null && featuredImage.Length > 0)
            {
                Console.WriteLine($"New image uploaded: {featuredImage.FileName}");
                
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "blog");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingPost.FeaturedImageUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingPost.FeaturedImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                        Console.WriteLine($"Deleted old image: {existingPost.FeaturedImageUrl}");
                    }
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + featuredImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await featuredImage.CopyToAsync(stream);
                }

                existingPost.FeaturedImageUrl = "/uploads/blog/" + uniqueFileName;
                Console.WriteLine($"Set new image: {existingPost.FeaturedImageUrl}");
            }
            else
            {
                Console.WriteLine("No new image uploaded, keeping existing");
                // Keep existing FeaturedImageUrl from database, not from form
                blogPost.FeaturedImageUrl = existingPost.FeaturedImageUrl;
            }

            // Update only the fields that should change
            existingPost.Title = blogPost.Title;
            existingPost.Content = blogPost.Content;
            existingPost.Excerpt = blogPost.Excerpt;
            existingPost.BlogCategoryId = blogPost.BlogCategoryId;
            existingPost.Tags = blogPost.Tags;
            existingPost.Slug = blogPost.Slug;
            existingPost.Author = blogPost.Author;
            existingPost.IsPublished = blogPost.IsPublished;
            existingPost.AllowComments = blogPost.AllowComments;
            existingPost.LastUpdated = DateTime.UtcNow;
            
            // If FeaturedImageUrl was updated, set it
            if (!string.IsNullOrEmpty(blogPost.FeaturedImageUrl))
            {
                existingPost.FeaturedImageUrl = blogPost.FeaturedImageUrl;
            }

            Console.WriteLine($"Updating blog post #{id}: {existingPost.Title}");
            
            _context.Update(existingPost);
            var result = await _context.SaveChangesAsync();
            
            Console.WriteLine($"Save result: {result} rows affected");

            TempData["Success"] = "Blog post updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR updating blog post: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            ModelState.AddModelError("", $"Error updating blog post: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("ModelState is INVALID - not saving");
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine($"Validation error: {error.ErrorMessage}");
        }
    }

    ViewBag.Categories = new SelectList(await _context.BlogCategories.ToListAsync(), "Id", "Name");
    return View(blogPost);
}

        // POST: AdminBlog/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                // Delete associated image
                if (!string.IsNullOrEmpty(blogPost.FeaturedImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", blogPost.FeaturedImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.BlogPosts.Remove(blogPost);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Blog post deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.Id == id);
        }

        private string GenerateSlug(string title)
        {
            return title.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("?", "")
                .Replace("!", "")
                .Replace("'", "")
                .Replace("\"", "");
        }
    }
}