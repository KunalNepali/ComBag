using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComBag.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        [Required]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Order Status")]
        public string Status { get; set; } = "Pending";

    // Payment Information
    [StringLength(20)]
    [Display(Name = "Payment Method")]
    public string PaymentMethod { get; set; } = "COD"; // COD, Esewa, Khalti, BankTransfer
    
    [Display(Name = "Payment Status")]
    [StringLength(20)]
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded
    
    [Display(Name = "Payment Transaction ID")]
    [StringLength(100)]
    public string? PaymentTransactionId { get; set; }
    
    [Display(Name = "Payment Date")]
    public DateTime? PaymentDate { get; set; }
        public string? Email { get; set; }
    public string? Phone { get; set; }
    

        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        [Display(Name = "City")]
        public string ShippingCity { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
        [Display(Name = "State/Province")]
        public string ShippingState { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        [Display(Name = "Postal Code")]
        public string ShippingPostalCode { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}