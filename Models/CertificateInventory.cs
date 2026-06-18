using System.ComponentModel.DataAnnotations;

namespace MAUN.Tomer.Web.Models;

public class CertificateInventory
{
    public int CertificateId { get; set; }

    [Display(Name = "Sertifika Tarihi")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "Sertifika tarihi zorunludur.")]
    public DateTime CertificateDate { get; set; } = DateTime.Today;

    [Display(Name = "T.C. Kimlik / Pasaport No")]
    [Required(ErrorMessage = "Kimlik veya pasaport numarası zorunludur.")]
    [StringLength(50)]
    public string IdentityOrPassportNo { get; set; } = "";

    [Display(Name = "Ad Soyad")]
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(255)]
    public string FullName { get; set; } = "";

    [Display(Name = "Sertifika No")]
    [StringLength(100)]
    public string? CertificateNo { get; set; }

    [Display(Name = "Seviye")]
    [Required(ErrorMessage = "Seviye zorunludur.")]
    [StringLength(50)]
    public string Level { get; set; } = "";

    [Display(Name = "Okuma Puanı")]
    [Range(0, 100)]
    public int ReadingScore { get; set; }

    [Display(Name = "Yazma Puanı")]
    [Range(0, 100)]
    public int WritingScore { get; set; }

    [Display(Name = "Dinleme Puanı")]
    [Range(0, 100)]
    public int ListeningScore { get; set; }

    [Display(Name = "Konuşma Puanı")]
    [Range(0, 100)]
    public int SpeakingScore { get; set; }

    [Display(Name = "Toplam Puan")]
    public int? TotalScore { get; set; }

    [Display(Name = "Başarı Durumu")]
    [StringLength(100)]
    public string? PassingStatus { get; set; }
}
