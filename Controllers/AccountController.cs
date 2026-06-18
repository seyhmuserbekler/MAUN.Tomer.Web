using System.Security.Claims;
using MAUN.Tomer.Web.Data;
using MAUN.Tomer.Web.Infrastructure;
using MAUN.Tomer.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MAUN.Tomer.Web.Controllers;

[Route("Admin")]
public class AccountController : Controller
{
    private readonly IAdminUserRepository adminUsers;

    public AccountController(IAdminUserRepository adminUsers)
    {
        this.adminUsers = adminUsers;
    }

    [AllowAnonymous]
    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var admin = await adminUsers.FindByUsernameAsync(model.Username);
        if (admin is null || !admin.IsActive || !PasswordHasher.VerifyPassword(model.Password, admin.PasswordHash, admin.PasswordSalt))
        {
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.AdminUserId.ToString()),
            new(ClaimTypes.Name, admin.FullName),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                IssuedUtc = DateTimeOffset.UtcNow
            });

        await adminUsers.UpdateLastLoginAsync(admin.AdminUserId);
        return RedirectToLocal(model.ReturnUrl);
    }

    [Authorize]
    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "CertificateValidation");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Certificates");
    }
}
