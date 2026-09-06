using Onudhabon.Models;

namespace Onudhabon_ISD.Models
{
    public class AuthViewModel
    {
        public LoginViewModel Login { get; set; } = new LoginViewModel();
        public RegisterViewModel Register { get; set; } = new RegisterViewModel();
        public string ActiveTab { get; set; } = "login"; 
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}