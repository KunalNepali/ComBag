using System.ComponentModel.DataAnnotations;

namespace ComBag.Models
{
    public class ServiceInquiry
    {
        public int Id { get; set; }

        // Customer Information
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        // Service Information
        public int? RepairServiceId { get; set; }
        public RepairService? RepairService { get; set; }

        [StringLength(100, ErrorMessage = "Bag type cannot exceed 100 characters")]
        [Display(Name = "Bag Type/Brand")]
        public string? BagType { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        [Display(Name = "Description of Damage/Customization")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Damage Image URL")]
        public string? DamageImageUrl { get; set; }

        // Status Tracking
        [Display(Name = "Status")]
        [StringLength(50)]
        public string Status { get; set; } = "New"; // New, In Progress, Quoted, Completed, Cancelled

        [Display(Name = "Admin Notes")]
        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string? AdminNotes { get; set; }

        [Display(Name = "Quoted Price")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? QuotedPrice { get; set; }

        [Display(Name = "Estimated Completion Date")]
        [DataType(DataType.Date)]
        public DateTime? EstimatedCompletionDate { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}