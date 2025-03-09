using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Parolanız eşleşmiyor")]
        public string? NewPasswordConfirm { get; set; }
    }
}
