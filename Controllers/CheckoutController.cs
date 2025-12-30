using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComBag.Data;
using ComBag.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using ComBag.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ComBag.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CheckoutController> _logger;
        private readonly ILoggerFactory _loggerFactory; // Add this
        private const string CartSessionKey = "ShoppingCart";

        public CheckoutController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<CheckoutController> logger,
            ILoggerFactory loggerFactory) // Add this parameter
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _loggerFactory = loggerFactory;
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
            Console.WriteLine($"Payment Method: {model.PaymentMethod}");
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
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = "Pending",
                    ShippingAddress = model.ShippingAddress,
                    ShippingCity = model.ShippingCity,
                    ShippingState = model.ShippingState,
                    ShippingPostalCode = model.ShippingPostalCode,
                    Email = user.Email,
                    Phone = user.PhoneNumber
                };

                Console.WriteLine($"📦 Creating order with total: ${order.TotalAmount}");
                
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Get Order ID
                Console.WriteLine($"✅ Order saved with ID: {order.Id}");

                // Add initial tracking entry - USE SINGULAR 'OrderTracking' (check your DbSet name)
                var tracking = new OrderTracking
{
    OrderId = order.Id,
    Status = "Pending",
    Description = "Order placed successfully",
    CreatedBy = "System",
    Location = "Order Placed",
    Carrier = "Not Assigned",        // ADD THIS
    TrackingNumber = "N/A",          // ADD THIS - required field
    NotifyCustomer = true
};
                
                // Try using the singular name since your ApplicationDbContext shows 'OrderTracking'
                // If this doesn't work, check what your actual DbSet name is
                _context.OrderTracking.Add(tracking);

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

                // Handle payment based on method
                if (model.PaymentMethod != "COD")
                {
                    return await HandleOnlinePayment(order, model.PaymentMethod);
                }

                // Clear cart
                ClearCart();
                Console.WriteLine($"🛒 Cart cleared");

                TempData["Success"] = $"Order #{order.Id} placed successfully!";
                Console.WriteLine("✅ ORDER PLACED SUCCESSFULLY!");
                
                return RedirectToAction("Details", "Orders", new { id = order.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPTION: {ex.Message}");
                Console.WriteLine($"   Inner: {ex.InnerException?.Message}");
                
                ModelState.AddModelError("", $"Error placing order: {ex.Message}");
                
                // Return to checkout with current data
                model.CartItems = cart;
                model.TotalAmount = cart.Sum(item => item.Subtotal);
                model.User = user;
                return View("Index", model);
            }
        }

        private async Task<IActionResult> HandleOnlinePayment(Order order, string paymentMethod)
        {
            // SIMPLIFIED VERSION - Just show message for now
            TempData["Error"] = $"Online payment via {paymentMethod} is not yet implemented. Please use COD (Cash on Delivery) for now.";
            return RedirectToAction("Index", "Checkout");
            
            /* UNCOMMENT THIS WHEN READY FOR PAYMENT IMPLEMENTATION
            IPaymentService paymentService = paymentMethod switch
            {
                "Esewa" => new EsewaPaymentService(_configuration, _httpClientFactory, 
                    _loggerFactory.CreateLogger<EsewaPaymentService>()),
                "Khalti" => new KhaltiPaymentService(_configuration, _httpClientFactory, 
                    _loggerFactory.CreateLogger<KhaltiPaymentService>()),
                "BankTransfer" => new BankTransferPaymentService(_configuration, 
                    _loggerFactory.CreateLogger<BankTransferPaymentService>()),
                _ => throw new ArgumentException("Invalid payment method")
            };

            var returnUrl = Url.Action("PaymentCallback", "Checkout", new { orderId = order.Id }, Request.Scheme);
            var paymentResponse = await paymentService.InitiatePaymentAsync(order, returnUrl);
            
            if (paymentResponse.Success)
            {
                // Store payment info
                order.PaymentTransactionId = paymentResponse.TransactionId;
                await _context.SaveChangesAsync();

                // Redirect to payment gateway
                return Redirect(paymentResponse.PaymentUrl);
            }
            else
            {
                TempData["Error"] = $"Payment initiation failed: {paymentResponse.Message}";
                return RedirectToAction("Index", "Checkout");
            }
            */
        }

        // Payment callback handler
        public async Task<IActionResult> PaymentCallback(int orderId, string status, string transactionId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            if (status == "success")
            {
                // Verify payment
                IPaymentService paymentService = order.PaymentMethod switch
                {
                    "Esewa" => new EsewaPaymentService(_configuration, _httpClientFactory, 
                        _loggerFactory.CreateLogger<EsewaPaymentService>()),
                    "Khalti" => new KhaltiPaymentService(_configuration, _httpClientFactory, 
                        _loggerFactory.CreateLogger<KhaltiPaymentService>()),
                    _ => null
                };

                if (paymentService != null)
                {
                    var verification = await paymentService.VerifyPaymentAsync(transactionId);
                    
                    if (verification.Success)
                    {
                        order.PaymentStatus = "Paid";
                        order.PaymentDate = DateTime.UtcNow;
                        order.Status = "Processing";
                        
                        var tracking = new OrderTracking
{
    OrderId = order.Id,
    Status = "Pending",
    Description = "Order placed successfully",
    CreatedBy = "System",
    Location = "Order Placed",
    Carrier = "Not Assigned",        // ADD THIS
    TrackingNumber = "N/A",          // ADD THIS - required field
    NotifyCustomer = true
};
                        _context.OrderTracking.Add(tracking); // Use singular
                        
                        await _context.SaveChangesAsync();
                        
                        ClearCart();
                        TempData["Success"] = $"Order #{order.Id} placed successfully! Payment confirmed.";
                        return RedirectToAction("Details", "Orders", new { id = order.Id });
                    }
                }
            }

            // Payment failed
            order.PaymentStatus = "Failed";
            var failedTracking = new OrderTracking
            {
   OrderId = order.Id,
    Status = "Pending",
    Description = "Order placed successfully",
    CreatedBy = "System",
    Location = "Order Placed",
    Carrier = "Not Assigned",        // ADD THIS
    TrackingNumber = "N/A",          // ADD THIS - required field
    NotifyCustomer = true
            };
            _context.OrderTracking.Add(failedTracking); // Use singular
            
            await _context.SaveChangesAsync();
            
            TempData["Error"] = "Payment failed. Please try again.";
            return RedirectToAction("Index", "Checkout");
        }

        private List<CartItemViewModel> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return cartJson == null ? new List<CartItemViewModel>() 
                : JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
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
        public string ShippingAddress { get; set; } = "";
        
        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        [Display(Name = "City")]
        public string ShippingCity { get; set; } = "";
        
        [Required(ErrorMessage = "State is required")]
        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
        [Display(Name = "State/Province")]
        public string ShippingState { get; set; } = "";
        
        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        [Display(Name = "Postal Code")]
        public string ShippingPostalCode { get; set; } = "";
        
        [Required(ErrorMessage = "Please select a payment method")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "COD"; // Default to COD
        
        public bool SaveAddress { get; set; }
    }
}