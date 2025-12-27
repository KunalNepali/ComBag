using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComBag.Models
{
public class BlogPost
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Blog Title")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Content is required")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Content")]
    public string Content { get; set; }

    [StringLength(500)]
    [Display(Name = "Short Description")]
    public string? Excerpt { get; set; }

    // ✅ REMOVE [Required] or make nullable
    [StringLength(200)]
    [Display(Name = "Featured Image URL")]
    public string? FeaturedImageUrl { get; set; } // Add ? to make nullable

    [StringLength(100)]
    [Display(Name = "Author")]
    public string Author { get; set; } = "Admin";

    [Display(Name = "Publication Date")]
    public DateTime PublishedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [Display(Name = "Is Published")]
    public bool IsPublished { get; set; } = true;

    [Display(Name = "Allow Comments")]
    public bool AllowComments { get; set; } = true;

    [Display(Name = "Views")]
    public int ViewCount { get; set; } = 0;

    // ✅ CHANGE BlogCategoryId to nullable (int?)
    [Display(Name = "Category")]
    public int? BlogCategoryId { get; set; } // Change to int?

    [ForeignKey("BlogCategoryId")]
    public BlogCategory? BlogCategory { get; set; } // Make nullable

    // Tags as comma-separated string
    [StringLength(200)]
    [Display(Name = "Tags (comma separated)")]
    public string? Tags { get; set; }

    [Display(Name = "Slug (URL friendly)")]
    [StringLength(200)]
    public string? Slug { get; set; }
}
}