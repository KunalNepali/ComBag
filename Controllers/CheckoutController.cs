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

        public async Task<IActionResult> Index()
        {
            var cart = GetCartFromSession();
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
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
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var cart = GetCartFromSession();
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            
            // Update user information if provided
            if (!string.IsNullOrEmpty(model.ShippingAddress))
            {
                user.Address = model.ShippingAddress;
                user.City = model.ShippingCity;
                user.State = model.ShippingState;
                user.PostalCode = model.ShippingPostalCode;
                await _userManager.UpdateAsync(user);
            }

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

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items
            foreach (var cartItem in cart)
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null)
                {
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

            // Clear cart
            HttpContext.Session.Remove(CartSessionKey);

            TempData["Success"] = $"Order #{order.Id} placed successfully! You will pay ${order.TotalAmount:N2} on delivery.";
            return RedirectToAction("Details", "Orders", new { id = order.Id });
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
        public List<CartItemViewModel> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
        public ApplicationUser User { get; set; }
        
        [Required]
        public string ShippingAddress { get; set; }
        
        [Required]
        public string ShippingCity { get; set; }
        
        [Required]
        public string ShippingState { get; set; }
        
        [Required]
        public string ShippingPostalCode { get; set; }
        
        public bool SaveAddress { get; set; }
    }
}