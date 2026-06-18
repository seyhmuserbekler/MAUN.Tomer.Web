using System.ComponentModel.DataAnnotations;

namespace MAUN.Tomer.Web.Models;

public class CertificateSearchViewModel
{
    [Display(Name = "T.C. Kimlik / Pasaport No")]
    [Required(ErrorMessage = "Kimlik veya pasaport numarası giriniz.")]
    public string IdentityOrPassportNo { get; set; } = "";

    public IReadOnlyList<CertificateInventory>? Results { get; set; }

    public bool SearchCompleted { get; set; }
}
