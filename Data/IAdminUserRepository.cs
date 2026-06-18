using MAUN.Tomer.Web.Models;

namespace MAUN.Tomer.Web.Data;

public interface IAdminUserRepository
{
    Task<IReadOnlyList<AdminUser>> ListAsync();
    Task<AdminUser?> GetAsync(int adminUserId);
    Task<AdminUser?> FindByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username, int exceptAdminUserId = 0);
    Task<int> CreateAsync(AdminUser user);
    Task UpdateAsync(AdminUser user, bool updatePassword);
    Task UpdateLastLoginAsync(int adminUserId);
}
