using Microsoft.AspNetCore.Identity;
using ComBag.Models;
using Microsoft.EntityFrameworkCore;

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
            // 1. PRODUCT CATEGORIES
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

            // 2. BLOG CATEGORIES
            if (!context.BlogCategories.Any())
            {
                var blogCategories = new List<BlogCategory>
                {
                    new BlogCategory { Name = "Information", Description = "General information and updates", Slug = "information" },
                    new BlogCategory { Name = "Repairing", Description = "Bag repair tips and services", Slug = "repairing" },
                    new BlogCategory { Name = "Bags", Description = "All about bags", Slug = "bags" },
                    new BlogCategory { Name = "Belts", Description = "Belt styles and care", Slug = "belts" },
                    new BlogCategory { Name = "Luggages", Description = "Luggage and travel bags", Slug = "luggages" },
                    new BlogCategory { Name = "Customized", Description = "Custom bag designs", Slug = "customized" },
                    new BlogCategory { Name = "School", Description = "School bags and accessories", Slug = "school" },
                    new BlogCategory { Name = "Laptop", Description = "Laptop bags and sleeves", Slug = "laptop" }
                };

                await context.BlogCategories.AddRangeAsync(blogCategories);
                await context.SaveChangesAsync();
                
                // 3. SAMPLE BLOG POSTS (Optional)
                if (!context.BlogPosts.Any())
                {
                    var blogPosts = new List<BlogPost>
                    {
                        new BlogPost
                        {
                            Title = "Welcome to ComBag Blog",
                            Content = "This is our first blog post about bags and accessories.",
                            Excerpt = "Welcome to our blog where we share tips about bags.",
                            Author = "Admin",
                            Slug = "welcome-to-combag-blog",
                            BlogCategoryId = blogCategories[0].Id, // Information category
                            IsPublished = true,
                            PublishedDate = DateTime.UtcNow.AddDays(-2),
                            LastUpdated = DateTime.UtcNow.AddDays(-2)
                        },
                        new BlogPost
                        {
                            Title = "How to Care for Your Leather Bag",
                            Content = "Tips for maintaining leather bags...",
                            Excerpt = "Learn how to care for your leather bags properly.",
                            Author = "Admin",
                            Slug = "how-to-care-for-leather-bag",
                            BlogCategoryId = blogCategories[1].Id, // Repairing category
                            IsPublished = true,
                            PublishedDate = DateTime.UtcNow.AddDays(-1),
                            LastUpdated = DateTime.UtcNow.AddDays(-1)
                        }
                    };

                    await context.BlogPosts.AddRangeAsync(blogPosts);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}