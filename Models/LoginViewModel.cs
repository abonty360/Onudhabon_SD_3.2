using System.ComponentModel.DataAnnotations;

namespace Onudhabon.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your username or email")]
        [Display(Name = "Username or Email")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }

        public string Role { get; set; } = "Patient";
    }
}