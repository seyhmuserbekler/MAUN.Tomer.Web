using MAUN.Tomer.Web.Models;

namespace MAUN.Tomer.Web.Data;

public interface ICertificateRepository
{
    Task<IReadOnlyList<CertificateInventory>> ListAsync(string? search);
    Task<IReadOnlyList<CertificateInventory>> FindByIdentityAsync(string identityOrPassportNo);
    Task<CertificateInventory?> GetAsync(int id);
    Task<bool> HasDuplicateAsync(CertificateInventory certificate);
    Task<int> CreateAsync(CertificateInventory certificate);
    Task UpdateAsync(CertificateInventory certificate);
    Task DeleteAsync(int id);
}
