using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComBag.Data;
using ComBag.Models;

namespace ComBag.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string search = "")
        {
            IQueryable<Product> productsQuery = _context.Products
                .Include(p => p.Category);

            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => 
                    p.Name.Contains(search) || 
                    p.Description.Contains(search));
            }

            var products = await productsQuery.ToListAsync();
            ViewBag.Search = search;
            
            return View(products);
        }

        // GET: Products/Details/5
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
    }
}