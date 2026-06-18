using MAUN.Tomer.Web.Data;
using MAUN.Tomer.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace MAUN.Tomer.Web.Controllers;

[Authorize]
[Route("Admin/Certificates")]
public class CertificatesController : Controller
{
    private const int PassingScore = 60;
    private readonly ICertificateRepository repository;

    public CertificatesController(ICertificateRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Search = search;
        return View(await repository.ListAsync(search));
    }

    [HttpGet("Export")]
    public async Task<IActionResult> Export(string? search)
    {
        var certificates = await repository.ListAsync(search);
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sertifikalar");

        var headers = new[]
        {
            "Sertifika Tarihi", "Kimlik/Pasaport No", "Ad Soyad", "Sertifika No", "Seviye",
            "Okuma", "Yazma", "Dinleme", "Konuşma", "Toplam", "Başarı Durumu"
        };

        for (var column = 0; column < headers.Length; column++)
        {
            worksheet.Cells[1, column + 1].Value = headers[column];
            worksheet.Cells[1, column + 1].Style.Font.Bold = true;
        }

        for (var row = 0; row < certificates.Count; row++)
        {
            var certificate = certificates[row];
            var excelRow = row + 2;

            worksheet.Cells[excelRow, 1].Value = certificate.CertificateDate;
            worksheet.Cells[excelRow, 1].Style.Numberformat.Format = "dd.mm.yyyy";
            worksheet.Cells[excelRow, 2].Value = certificate.IdentityOrPassportNo;
            worksheet.Cells[excelRow, 3].Value = certificate.FullName;
            worksheet.Cells[excelRow, 4].Value = certificate.CertificateNo;
            worksheet.Cells[excelRow, 5].Value = certificate.Level;
            worksheet.Cells[excelRow, 6].Value = certificate.ReadingScore;
            worksheet.Cells[excelRow, 7].Value = certificate.WritingScore;
            worksheet.Cells[excelRow, 8].Value = certificate.ListeningScore;
            worksheet.Cells[excelRow, 9].Value = certificate.SpeakingScore;
            worksheet.Cells[excelRow, 10].Value = certificate.TotalScore;
            worksheet.Cells[excelRow, 11].Value = certificate.PassingStatus;
        }

        if (worksheet.Dimension is not null)
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        var fileName = $"MAUN_Tomer_Sertifikalar_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View("Edit", new CertificateInventory());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CertificateInventory certificate)
    {
        CalculateCertificateResult(certificate);

        if (!ModelState.IsValid)
            return View("Edit", certificate);

        if (await repository.HasDuplicateAsync(certificate))
        {
            ModelState.AddModelError("", "Bu sertifika numarası veya aynı kişi/tarih/seviye bilgisiyle kayıt zaten var.");
            return View("Edit", certificate);
        }

        await repository.CreateAsync(certificate);
        TempData["Message"] = "Sertifika kaydı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var certificate = await repository.GetAsync(id);
        return certificate is null ? NotFound() : View(certificate);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CertificateInventory certificate)
    {
        certificate.CertificateId = id;
        CalculateCertificateResult(certificate);

        if (!ModelState.IsValid)
            return View(certificate);

        if (await repository.HasDuplicateAsync(certificate))
        {
            ModelState.AddModelError("", "Bu sertifika numarası veya aynı kişi/tarih/seviye bilgisiyle kayıt zaten var.");
            return View(certificate);
        }

        await repository.UpdateAsync(certificate);
        TempData["Message"] = "Sertifika kaydı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await repository.DeleteAsync(id);
        TempData["Message"] = "Sertifika kaydı silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Import")]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost("Import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError("", "Excel dosyası seçiniz.");
            return View();
        }

        var imported = 0;
        var errors = new List<string>();
        using var package = new ExcelPackage(file.OpenReadStream());
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet?.Dimension is null)
        {
            ModelState.AddModelError("", "Excel dosyasında okunabilir veri bulunamadı.");
            return View();
        }

        for (var row = 2; row <= worksheet.Dimension.End.Row; row++)
        {
            var certificate = new CertificateInventory
            {
                CertificateDate = ReadDate(worksheet.Cells[row, 1].Value),
                IdentityOrPassportNo = ReadText(worksheet.Cells[row, 2].Value),
                FullName = ReadText(worksheet.Cells[row, 3].Value),
                CertificateNo = ReadText(worksheet.Cells[row, 4].Value),
                Level = ReadText(worksheet.Cells[row, 5].Value),
                ReadingScore = ReadInt(worksheet.Cells[row, 6].Value),
                WritingScore = ReadInt(worksheet.Cells[row, 7].Value),
                ListeningScore = ReadInt(worksheet.Cells[row, 8].Value),
                SpeakingScore = ReadInt(worksheet.Cells[row, 9].Value)
            };

            if (string.IsNullOrWhiteSpace(certificate.IdentityOrPassportNo) ||
                string.IsNullOrWhiteSpace(certificate.FullName) ||
                string.IsNullOrWhiteSpace(certificate.Level))
            {
                errors.Add($"Satır {row}: Kimlik/Pasaport No, Ad Soyad ve Seviye zorunludur.");
                continue;
            }

            CalculateCertificateResult(certificate);

            if (await repository.HasDuplicateAsync(certificate))
            {
                errors.Add($"Satır {row}: Mükerrer kayıt atlandı.");
                continue;
            }

            await repository.CreateAsync(certificate);
            imported++;
        }

        TempData["Message"] = $"{imported} sertifika kaydı içe aktarıldı.";
        if (errors.Count > 0)
            TempData["Warning"] = string.Join(" ", errors.Take(20)) + (errors.Count > 20 ? $" {errors.Count - 20} ek hata daha var." : "");

        return RedirectToAction(nameof(Index));
    }

    private static void CalculateCertificateResult(CertificateInventory certificate)
    {
        certificate.TotalScore = certificate.ReadingScore + certificate.WritingScore + certificate.ListeningScore + certificate.SpeakingScore;
        certificate.PassingStatus = certificate.TotalScore > PassingScore && certificate.Level.Trim() != "0" ? "BAŞARILI" : "BAŞARISIZ";
    }

    private static string ReadText(object? value)
    {
        return value?.ToString()?.Trim() ?? "";
    }

    private static int ReadInt(object? value)
    {
        return int.TryParse(ReadText(value), out var number) ? number : 0;
    }

    private static DateTime ReadDate(object? value)
    {
        if (value is DateTime date)
            return date;

        if (double.TryParse(ReadText(value), out var oaDate))
            return DateTime.FromOADate(oaDate);

        return DateTime.TryParse(ReadText(value), out var parsed) ? parsed : DateTime.Today;
    }
}
