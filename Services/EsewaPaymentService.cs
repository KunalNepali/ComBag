using System.Text;
using System.Text.Json;
using ComBag.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ComBag.Services
{
    public class EsewaPaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EsewaPaymentService> _logger;

        public EsewaPaymentService(
            IConfiguration configuration, 
            IHttpClientFactory httpClientFactory,
            ILogger<EsewaPaymentService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<PaymentInitiationResponse> InitiatePaymentAsync(Order order, string returnUrl)
        {
            try
            {
                // For development, use test credentials
                // In production, use actual credentials from configuration
                var esewaConfig = _configuration.GetSection("PaymentGateways:Esewa");
                
                var requestData = new
                {
                    amount = order.TotalAmount.ToString("0.00"),
                    tax_amount = "0",
                    total_amount = order.TotalAmount.ToString("0.00"),
                    transaction_uuid = Guid.NewGuid().ToString(),
                    product_code = "EPAYTEST", // Use "EPAY" for production
                    success_url = returnUrl,
                    failure_url = returnUrl + "?status=failed",
                    product_service_charge = "0",
                    product_delivery_charge = "0",
                    signed_field_names = "total_amount,transaction_uuid,product_code",
                    signature = "" // You need to generate HMAC signature
                };

                // Generate signature (simplified - implement proper HMAC SHA256)
                var signatureData = $"total_amount={requestData.total_amount},transaction_uuid={requestData.transaction_uuid},product_code={requestData.product_code}";
                var signature = GenerateSignature(signatureData);
                
                // Create new object with signature (since anonymous types are immutable)
                var requestDataWithSignature = new
                {
                    amount = requestData.amount,
                    tax_amount = requestData.tax_amount,
                    total_amount = requestData.total_amount,
                    transaction_uuid = requestData.transaction_uuid,
                    product_code = requestData.product_code,
                    success_url = requestData.success_url,
                    failure_url = requestData.failure_url,
                    product_service_charge = requestData.product_service_charge,
                    product_delivery_charge = requestData.product_delivery_charge,
                    signed_field_names = requestData.signed_field_names,
                    signature = signature
                };

                return new PaymentInitiationResponse
                {
                    Success = true,
                    PaymentUrl = "https://rc-epay.esewa.com.np/api/epay/main/v2/form", // Test URL
                    TransactionId = requestData.transaction_uuid,
                    Message = "Payment initiated"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating eSewa payment");
                return new PaymentInitiationResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<PaymentVerificationResponse> VerifyPaymentAsync(string paymentId)
        {
            try
            {
                // Verify with eSewa API
                var client = _httpClientFactory.CreateClient();
                var esewaConfig = _configuration.GetSection("PaymentGateways:Esewa");
                
                var verificationUrl = "https://rc-epay.esewa.com.np/api/epay/transaction/status"; // Test URL
                
                var requestData = new
                {
                    product_code = "EPAYTEST",
                    transaction_uuid = paymentId
                };

                var response = await client.PostAsJsonAsync(verificationUrl, requestData);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var verificationResult = JsonSerializer.Deserialize<EsewaVerificationResponse>(responseContent);
                    
                    return new PaymentVerificationResponse
                    {
                        Success = verificationResult?.Status == "COMPLETE",
                        Status = verificationResult?.Status == "COMPLETE" ? "Paid" : "Pending",
                        TransactionId = paymentId,
                        Message = verificationResult?.Message ?? "Verification completed"
                    };
                }

                return new PaymentVerificationResponse
                {
                    Success = false,
                    Status = "Failed",
                    TransactionId = paymentId,
                    Message = "Verification failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying eSewa payment");
                return new PaymentVerificationResponse
                {
                    Success = false,
                    Status = "Failed",
                    TransactionId = paymentId,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private string GenerateSignature(string data)
        {
            // Implement HMAC SHA256 signature generation
            // This is a placeholder - implement proper signature generation
            // For actual implementation, you'll need the secret key from eSewa
            var secretKey = _configuration["PaymentGateways:Esewa:SecretKey"] ?? "test_secret";
            
            // Example of HMAC SHA256 implementation (simplified)
            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }
    }

    public class EsewaVerificationResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TransactionCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string TransactionUuid { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
    }
}