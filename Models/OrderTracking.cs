using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComBag.Models
{
    public class OrderTracking
    {
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled
        
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(100)]
        [Display(Name = "Location")]
        public string Location { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Tracking Number")]
        public string TrackingNumber { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Carrier")]
        public string Carrier { get; set; } // DHL, FedEx, Local Post, etc.
        
        [Display(Name = "Estimated Delivery")]
        public DateTime? EstimatedDelivery { get; set; }
        
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = "System"; // System, Admin, Customer
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Notify Customer")]
        public bool NotifyCustomer { get; set; } = true;
    }
}