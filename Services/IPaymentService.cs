namespace ComBag.Services
{
    public interface IPaymentService
    {
        Task<PaymentInitiationResponse> InitiatePaymentAsync(Order order, string returnUrl);
        Task<PaymentVerificationResponse> VerifyPaymentAsync(string paymentId);
    }

    public class PaymentInitiationResponse
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }

    public class PaymentVerificationResponse
    {
        public bool Success { get; set; }
        public string Status { get; set; } // Paid, Pending, Failed
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }
}