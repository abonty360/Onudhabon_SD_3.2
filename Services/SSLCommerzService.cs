using System.Globalization;
using System.Text.Json;
using Onudhabon.Models;

namespace Onudhabon.Services
{
    public class SSLCommerzService : ISSLCommerzService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<SSLCommerzService> _logger;

        private readonly string _storeId;
        private readonly string _storePassword;
        private readonly bool _isSandbox;
        private readonly string _initiateUrl;
        private readonly string _validationUrl;

        public SSLCommerzService(HttpClient httpClient, IConfiguration config, ILogger<SSLCommerzService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;

            _storeId = Environment.GetEnvironmentVariable("SSLC_STORE_ID") 
                ?? _config["PaymentGateway:StoreId"] 
                ?? "testbox";

            _storePassword = Environment.GetEnvironmentVariable("SSLC_STORE_PASSWORD") 
                ?? _config["PaymentGateway:StorePassword"] 
                ?? "qwerty";

            var sandboxStr = Environment.GetEnvironmentVariable("SSLC_IS_SANDBOX") 
                ?? _config["PaymentGateway:IsSandbox"] 
                ?? "true";
            _isSandbox = bool.TryParse(sandboxStr, out bool sb) ? sb : true;

            _initiateUrl = _isSandbox
                ? "https://sandbox.sslcommerz.com/gwprocess/v4/api.php"
                : "https://securepay.sslcommerz.com/gwprocess/v4/api.php";

            _validationUrl = _isSandbox
                ? "https://sandbox.sslcommerz.com/validator/api/validationserverAPI.php"
                : "https://securepay.sslcommerz.com/validator/api/validationserverAPI.php";
        }

        public async Task<SSLCommerzInitResponse?> InitiatePaymentAsync(Donation donation, string hostUrl)
        {
            try
            {
                string cleanHost = hostUrl.TrimEnd('/');
                string tranId = $"ONUD-{donation.Id}-{DateTime.UtcNow.Ticks % 1000000}";

                var postData = new Dictionary<string, string>
                {
                    { "store_id", _storeId },
                    { "store_passwd", _storePassword },
                    { "total_amount", donation.Amount.ToString("0.00", CultureInfo.InvariantCulture) },
                    { "currency", "BDT" },
                    { "tran_id", tranId },
                    { "success_url", $"{cleanHost}/Donation/PaymentSuccess" },
                    { "fail_url", $"{cleanHost}/Donation/PaymentFail" },
                    { "cancel_url", $"{cleanHost}/Donation/PaymentCancel" },
                    { "ipn_url", $"{cleanHost}/Donation/PaymentIpn" },
                    { "cus_name", string.IsNullOrWhiteSpace(donation.DonorName) ? "Anonymous Donor" : donation.DonorName },
                    { "cus_email", string.IsNullOrWhiteSpace(donation.DonorEmail) ? "donor@onudhabon.org" : donation.DonorEmail },
                    { "cus_add1", "Dhaka, Bangladesh" },
                    { "cus_city", "Dhaka" },
                    { "cus_country", "Bangladesh" },
                    { "cus_phone", string.IsNullOrWhiteSpace(donation.DonorPhone) ? "01711111111" : donation.DonorPhone },
                    { "shipping_method", "NO" },
                    { "product_name", $"Donation - {donation.Purpose}" },
                    { "product_category", "Philanthropy & Education" },
                    { "product_profile", "non-physical-goods" },
                    { "cus_postcode", "1200" },
                    { "num_of_item", "1" },
                    { "emi_option", "0" }
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, _initiateUrl);
                request.Content = new FormUrlEncodedContent(postData);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("SSLCommerz Init failed. HTTP {StatusCode}: {Content}", response.StatusCode, content);
                    return null;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<SSLCommerzInitResponse>(content, options);

                if (result != null)
                {
                    result.TranId = tranId;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while initiating SSLCommerz payment for donation {DonationId}", donation.Id);
                return null;
            }
        }

        public async Task<SSLCommerzValidationResponse?> ValidatePaymentAsync(string valId)
        {
            try
            {
                string queryUrl = $"{_validationUrl}?val_id={Uri.EscapeDataString(valId)}&store_id={Uri.EscapeDataString(_storeId)}&store_passwd={Uri.EscapeDataString(_storePassword)}&v=1&format=json";

                var response = await _httpClient.GetAsync(queryUrl);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("SSLCommerz Validate failed. HTTP {StatusCode}: {Content}", response.StatusCode, content);
                    return null;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<SSLCommerzValidationResponse>(content, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while validating SSLCommerz payment with val_id {ValId}", valId);
                return null;
            }
        }
    }
}
