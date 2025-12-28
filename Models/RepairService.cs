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
        public string Description { get; set; }

        [Required(ErrorMessage = "Starting price is required")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Starting Price")]
        public decimal StartingPrice { get; set; }

        [Range(0.5, 100, ErrorMessage = "Time must be between 0.5 and 100 hours")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Estimated Time (hours)")]     
        public decimal? EstimatedTimeHours { get; set; }


        [Required(ErrorMessage = "Price range is required")]
        [StringLength(100, ErrorMessage = "Price range cannot exceed 100 characters")]
        [Display(Name = "Estimated Price Range")]
        public string EstimatedPriceRange { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [StringLength(50, ErrorMessage = "Duration cannot exceed 50 characters")]
        [Display(Name = "Estimated Duration")]
        public string Duration { get; set; }
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Add these properties that are referenced in the view
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
        public ICollection<ServiceInquiry> ServiceInquiries { get; set; } = new List<ServiceInquiry>();

    }
}