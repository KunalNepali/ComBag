using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComBag.Data;
using ComBag.Models;
using System.Linq;

namespace ComBag.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Blog
        public async Task<IActionResult> Index(int? categoryId, string tag = null, int page = 1)
        {
            var pageSize = 6;
            
            IQueryable<BlogPost> query = _context.BlogPosts
                .Include(b => b.BlogCategory)
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishedDate);

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.BlogCategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(b => b.Tags != null && b.Tags.Contains(tag));
            }

            var totalPosts = await query.CountAsync();
            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = await _context.BlogCategories.ToListAsync();
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentTag = tag;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

            return View(posts);
        }

        // GET: /Blog/{slug}
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                .Include(b => b.BlogCategory)
                .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished);

            if (blogPost == null)
            {
                return NotFound();
            }

            // Increment view count
            blogPost.ViewCount++;
            await _context.SaveChangesAsync();

            ViewBag.RelatedPosts = await _context.BlogPosts
                .Where(b => b.BlogCategoryId == blogPost.BlogCategoryId && b.Id != blogPost.Id && b.IsPublished)
                .OrderByDescending(b => b.PublishedDate)
                .Take(3)
                .ToListAsync();

            // ✅ FIXED: Add the missing return statement
            ViewBag.Categories = await _context.BlogCategories.ToListAsync();
            return View(blogPost);
        }

        // POST: /Blog/UpdateViewCount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateViewCount(string slug)
        {
            var blogPost = await _context.BlogPosts
                .FirstOrDefaultAsync(b => b.Slug == slug);
    
            if (blogPost != null)
            {
                blogPost.ViewCount++;
                await _context.SaveChangesAsync();
            }   
            return Ok(); 
        }
    }
}