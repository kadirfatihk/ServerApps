using System.ComponentModel.DataAnnotations;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "E-posta adresi gerekli")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
    public string Email { get; set; }

    [Required]
    public string Token { get; set; }

    [Required(ErrorMessage = "Yeni şifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Yeni şifre tekrarı gereklidir")]
    [Compare("NewPassword", ErrorMessage = "Şifreler uyuşmuyor")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
}
