using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComBag.Data;
using ComBag.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
namespace ComBag.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CartSessionKey = "ShoppingCart";

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

[Authorize]
public async Task<IActionResult> Index()
{
    // Get current user
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }

    // Get cart from session
    var cart = GetCartFromSession();
    if (cart == null || !cart.Any())
    {
        TempData["Error"] = "Your cart is empty.";
        return RedirectToAction("Index", "Cart");
    }

    // Create view model
    var model = new CheckoutViewModel
    {
        CartItems = cart,
        TotalAmount = cart.Sum(item => item.Subtotal),
        User = user
    };

    return View(model);
}
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
{
    // === DEBUG LOGGING START ===
    Console.WriteLine("=== PLACE ORDER METHOD CALLED ===");
    Console.WriteLine($"Model is null: {model == null}");
    
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
        // Return to show validation errors
        var cart2 = GetCartFromSession();
        model.CartItems = cart2;
        model.TotalAmount = cart2?.Sum(item => item.Subtotal) ?? 0;
        model.User = await _userManager.GetUserAsync(User);
        return View("Index", model);
    }
    
    Console.WriteLine("✅ ModelState is VALID");
    Console.WriteLine($"Shipping Address: {model.ShippingAddress}");
    Console.WriteLine($"Shipping City: {model.ShippingCity}");
    Console.WriteLine($"Shipping State: {model.ShippingState}");
    Console.WriteLine($"Shipping Postal Code: {model.ShippingPostalCode}");
    // === DEBUG LOGGING END ===

    // Get cart from session
    var cart = GetCartFromSession();
    Console.WriteLine($"🛒 Cart items: {cart?.Count ?? 0}");
    
    if (cart == null || !cart.Any())
    {
        TempData["Error"] = "Your cart is empty.";
        return RedirectToAction("Index", "Cart");
    }

    // Get current user
    var user = await _userManager.GetUserAsync(User);
    Console.WriteLine($"👤 User: {user?.Email ?? "NULL"}");
    
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }

    try
    {
        // Create order
        var order = new Order
        {
            UserId = user.Id,
            OrderDate = DateTime.UtcNow,
            TotalAmount = cart.Sum(item => item.Subtotal),
            Status = "Pending",
            PaymentMethod = "COD",
            ShippingAddress = model.ShippingAddress,
            ShippingCity = model.ShippingCity,
            ShippingState = model.ShippingState,
            ShippingPostalCode = model.ShippingPostalCode
        };

        Console.WriteLine($"📦 Creating order with total: ${order.TotalAmount}");
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // Get Order ID
        Console.WriteLine($"✅ Order saved with ID: {order.Id}");

        // Create order items
        foreach (var cartItem in cart)
        {
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product != null)
            {
                Console.WriteLine($"   Adding: {product.Name} x {cartItem.Quantity} = ${cartItem.Subtotal}");
                
                // Reduce stock
                product.StockQuantity -= cartItem.Quantity;
                
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Price
                };
                _context.OrderItems.Add(orderItem);
            }
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"✅ Order items saved");

        // Clear cart
        HttpContext.Session.Remove(CartSessionKey);
        Console.WriteLine($"🛒 Cart cleared");

        TempData["Success"] = $"Order #{order.Id} placed successfully!";
        Console.WriteLine(" ORDER PLACED SUCCESSFULLY!");
        
        return RedirectToAction("Details", "Orders", new { id = order.Id });
    }
    catch (Exception ex)
    {
        Console.WriteLine($" EXCEPTION: {ex.Message}");
        Console.WriteLine($"   Inner: {ex.InnerException?.Message}");
        
        ModelState.AddModelError("", $"Error placing order: {ex.Message}");
        
        // Return to checkout with current data
        model.CartItems = cart;
        model.TotalAmount = cart.Sum(item => item.Subtotal);
        model.User = user;
        return View("Index", model);
    }
}

        private List<CartItemViewModel> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return cartJson == null ? new List<CartItemViewModel>() 
                : JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);
        }
    }

public class CheckoutViewModel
{
    public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
    public decimal TotalAmount { get; set; }
    public ApplicationUser? User { get; set; }
    
    [Required(ErrorMessage = "Shipping address is required")]
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    [Display(Name = "Shipping Address")]
    public string ShippingAddress { get; set; } = ""; // ← Add = ""
    
    [Required(ErrorMessage = "City is required")]
    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
    [Display(Name = "City")]
    public string ShippingCity { get; set; } = ""; // ← Add = ""
    
    [Required(ErrorMessage = "State is required")]
    [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
    [Display(Name = "State/Province")]
    public string ShippingState { get; set; } = ""; // ← Add = ""
    
    [Required(ErrorMessage = "Postal code is required")]
    [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
    [Display(Name = "Postal Code")]
    public string ShippingPostalCode { get; set; } = ""; // ← Add = ""
    
    public bool SaveAddress { get; set; }
}
}