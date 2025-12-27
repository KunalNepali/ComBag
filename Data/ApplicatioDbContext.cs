using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ComBag.Models;
using Microsoft.AspNetCore.Identity;

namespace ComBag.Data
{
    // ✅ Specify ApplicationUser as the generic parameter
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }        
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
           public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<BlogCategory> BlogCategories { get; set; }
    public DbSet<BlogComment> BlogComments { get; set; }
        // ✅ Optional: Override OnModelCreating if you need to configure relationships
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Important: Call base class method
               builder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
                
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        // Blog relationships

        builder.Entity<BlogPost>()
            .HasOne(p => p.BlogCategory)
            .WithMany(c => c.BlogPosts)
            .HasForeignKey(p => p.BlogCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.Entity<BlogComment>()
            .HasOne(c => c.BlogPost)
            .WithMany()
            .HasForeignKey(c => c.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<BlogComment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
            }
    }
}