namespace Onudhabon.Models
{
    public class AuthViewModel
    {
        public LoginViewModel Login { get; set; } = new LoginViewModel();
        public RegisterViewModel Register { get; set; } = new RegisterViewModel();
        public string ActiveTab { get; set; } = "login"; // "login" or "register"
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
