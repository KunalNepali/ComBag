namespace ComBag.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        
        // Can store either SessionId (for anonymous users) or UserId (for logged-in users)
        public string CartIdentifier { get; set; } // This will hold either SessionId or UserId
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}