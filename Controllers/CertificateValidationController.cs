using MAUN.Tomer.Web.Data;
using MAUN.Tomer.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MAUN.Tomer.Web.Controllers;

public class CertificateValidationController : Controller
{
    private readonly ICertificateRepository repository;

    public CertificateValidationController(ICertificateRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet("/")]
    [HttpGet("/CertificateValidation")]
    public IActionResult Index()
    {
        return View(new CertificateSearchViewModel());
    }

    [HttpPost("/CertificateValidation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CertificateSearchViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        model.Results = await repository.FindByIdentityAsync(model.IdentityOrPassportNo);
        model.SearchCompleted = true;
        return View(model);
    }
}
