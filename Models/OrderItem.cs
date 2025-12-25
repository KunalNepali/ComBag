using System.ComponentModel.DataAnnotations.Schema;

namespace ComBag.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        // Foreign keys
        public int OrderId { get; set; }
        public Order Order { get; set; }
        
        public int ProductId { get; set; }
        public Product Product { get; set; }
        
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        // Calculated property (not stored in DB)
        [NotMapped]
        public decimal Subtotal => Quantity * UnitPrice;
    }
}