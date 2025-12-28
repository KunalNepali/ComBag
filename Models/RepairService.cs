using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComBag.Models
{
    public class RepairService
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
        [Display(Name = "Service Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Price Range")]
        [StringLength(100, ErrorMessage = "Price range cannot exceed 100 characters")]
        public string? PriceRange { get; set; } // Changed from EstimatedPriceRange to PriceRange

        [Display(Name = "Duration")]
        [StringLength(50, ErrorMessage = "Duration cannot exceed 50 characters")]
        public string? Duration { get; set; } // Made nullable

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "Service Category")]
        [StringLength(50)]
        public string Category { get; set; } = "Repair";

        [Display(Name = "Icon Class")]
        [StringLength(50)]
        public string IconClass { get; set; } = "bi-tools";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<ServiceInquiry> ServiceInquiries { get; set; } = new List<ServiceInquiry>();
    }
}