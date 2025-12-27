using System.ComponentModel.DataAnnotations;

namespace ComBag.Models
{
    public class BlogCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Slug (URL friendly)")]
        [StringLength(100)]
        public string Slug { get; set; }

        // Navigation property
        public List<BlogPost> BlogPosts { get; set; }
    }
}