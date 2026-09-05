using System.ComponentModel.DataAnnotations;

namespace Onudhabon.Models
{
    /// <summary>
    /// Holds form state (UsernameOrEmail, Password, RememberMe, ReturnUrl)
    /// and triggers UI validation error messages.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your username or email address.")]
        [Display(Name = "Username or Email")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        /// <summary>
        /// Backward-compatible alias for UsernameOrEmail.
        /// </summary>
        public string EmailOrUsername
        {
            get => UsernameOrEmail;
            set => UsernameOrEmail = value;
        }

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
