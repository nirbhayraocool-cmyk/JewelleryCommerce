using System.ComponentModel.DataAnnotations;

namespace JewelleryCommerce.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}