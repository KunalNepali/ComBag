using Microsoft.AspNetCore.Identity;
using ComBag.Models;

namespace ComBag.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            context.Database.EnsureCreated();

            // Create Admin role if it doesn't exist
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create Customer role if it doesn't exist
            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            // Check if admin user exists
            var adminEmail = "admin@combag.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Create admin user
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin@123");

                if (result.Succeeded)
                {
                    // Assign admin role
                    await userManager.AddToRoleAsync(user, "Admin");
                    
                    // Also add sample categories and products
                    await SeedSampleData(context);
                }
            }
        }

        private static async Task SeedSampleData(ApplicationDbContext context)
        {
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Electronics", Description = "Electronic devices and gadgets" },
                    new Category { Name = "Clothing", Description = "Apparel and fashion items" },
                    new Category { Name = "Books", Description = "Books and educational materials" },
                    new Category { Name = "Home & Kitchen", Description = "Home appliances and kitchenware" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // Add sample products
                var products = new List<Product>
                {
                    new Product 
                    { 
                        Name = "Wireless Headphones", 
                        Description = "Noise-cancelling wireless headphones", 
                        Price = 99.99m, 
                        StockQuantity = 50,
                        CategoryId = categories[0].Id,
                        ImageUrl = "/images/headphones.jpg"
                    },
                    new Product 
                    { 
                        Name = "Smart Watch", 
                        Description = "Fitness tracking smart watch", 
                        Price = 199.99m, 
                        StockQuantity = 30,
                        CategoryId = categories[0].Id,
                        ImageUrl = "/images/smartwatch.jpg"
                    },
                    new Product 
                    { 
                        Name = "T-Shirt", 
                        Description = "100% Cotton T-Shirt", 
                        Price = 19.99m, 
                        StockQuantity = 100,
                        CategoryId = categories[1].Id,
                        ImageUrl = "/images/tshirt.jpg"
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}