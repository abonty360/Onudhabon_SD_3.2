using Onudhabon.Models;

namespace Onudhabon.Services
{
    public interface ISSLCommerzService
    {
        Task<SSLCommerzInitResponse?> InitiatePaymentAsync(Donation donation, string hostUrl);
        Task<SSLCommerzValidationResponse?> ValidatePaymentAsync(string valId);
    }
}
