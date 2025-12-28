using ComBag.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ComBag.Services
{
    public class BankTransferPaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BankTransferPaymentService> _logger;

        public BankTransferPaymentService(
            IConfiguration configuration,
            ILogger<BankTransferPaymentService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<PaymentInitiationResponse> InitiatePaymentAsync(Order order, string returnUrl)
        {
            // For bank transfer, just redirect to order page with instructions
            return Task.FromResult(new PaymentInitiationResponse
            {
                Success = true,
                PaymentUrl = returnUrl + "?method=bank-transfer",
                TransactionId = Guid.NewGuid().ToString(),
                Message = "Please complete bank transfer"
            });
        }

        public Task<PaymentVerificationResponse> VerifyPaymentAsync(string paymentId)
        {
            return Task.FromResult(new PaymentVerificationResponse
            {
                Success = true,
                Status = "Pending", // Admin will manually verify
                TransactionId = paymentId,
                Message = "Awaiting bank transfer confirmation"
            });
        }
    }
}