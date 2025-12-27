using System.ComponentModel.DataAnnotations;

namespace ComBag.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public List<Product>? Products { get; set; }
    }
}