using MAUN.Tomer.Web.Data;
using MAUN.Tomer.Web.Infrastructure;
using MAUN.Tomer.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MAUN.Tomer.Web.Controllers;

[Authorize]
[Route("Admin/Users")]
public class AdminUsersController : Controller
{
    private readonly IAdminUserRepository users;

    public AdminUsersController(IAdminUserRepository users)
    {
        this.users = users;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View(await users.ListAsync());
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View("Edit", new AdminUserEditViewModel { IsActive = true });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserEditViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Yeni kullanıcı için şifre zorunludur.");

        if (!ModelState.IsValid)
            return View("Edit", model);

        if (await users.UsernameExistsAsync(model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "Bu kullanıcı adı zaten kullanılıyor.");
            return View("Edit", model);
        }

        var password = PasswordHasher.HashPassword(model.Password!);
        await users.CreateAsync(new AdminUser
        {
            Username = model.Username,
            FullName = model.FullName,
            IsActive = model.IsActive,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt
        });

        TempData["Message"] = "Kullanıcı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await users.GetAsync(id);
        if (user is null)
            return NotFound();

        return View(new AdminUserEditViewModel
        {
            AdminUserId = user.AdminUserId,
            Username = user.Username,
            FullName = user.FullName,
            IsActive = user.IsActive
        });
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminUserEditViewModel model)
    {
        model.AdminUserId = id;

        if (!ModelState.IsValid)
            return View(model);

        var current = await users.GetAsync(id);
        if (current is null)
            return NotFound();

        if (await users.UsernameExistsAsync(model.Username, id))
        {
            ModelState.AddModelError(nameof(model.Username), "Bu kullanıcı adı zaten kullanılıyor.");
            return View(model);
        }

        current.Username = model.Username;
        current.FullName = model.FullName;
        current.IsActive = model.IsActive;

        var updatePassword = !string.IsNullOrWhiteSpace(model.Password);
        if (updatePassword)
        {
            var password = PasswordHasher.HashPassword(model.Password!);
            current.PasswordHash = password.Hash;
            current.PasswordSalt = password.Salt;
        }

        await users.UpdateAsync(current, updatePassword);
        TempData["Message"] = "Kullanıcı güncellendi.";
        return RedirectToAction(nameof(Index));
    }
}
