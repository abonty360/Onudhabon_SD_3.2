namespace Onudhabon.Services
{
    public interface IEmailValidationService
    {
        Task<(bool Exists, string? ErrorMessage)> ValidateEmailExistsAsync(string email);
    }
}
