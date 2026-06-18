using System.ComponentModel.DataAnnotations;

namespace MAUN.Tomer.Web.Models;

public class AdminUserEditViewModel
{
    public int AdminUserId { get; set; }

    [Display(Name = "Kullanıcı Adı")]
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(50)]
    public string Username { get; set; } = "";

    [Display(Name = "Ad Soyad")]
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(150)]
    public string FullName { get; set; } = "";

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Şifre")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string? Password { get; set; }
}
