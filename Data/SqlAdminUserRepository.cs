using MAUN.Tomer.Web.Models;
using Microsoft.Data.SqlClient;

namespace MAUN.Tomer.Web.Data;

public class SqlAdminUserRepository : IAdminUserRepository
{
    private readonly string connectionString;

    public SqlAdminUserRepository(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string bulunamadı.");
    }

    public async Task<IReadOnlyList<AdminUser>> ListAsync()
    {
        const string sql = """
            SELECT AdminUserId, Username, PasswordHash, PasswordSalt, FullName, IsActive, CreatedAt, LastLoginAt
            FROM dbo.AdminUsers
            ORDER BY IsActive DESC, FullName, Username
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        return await ReadListAsync(command);
    }

    public async Task<AdminUser?> GetAsync(int adminUserId)
    {
        const string sql = """
            SELECT AdminUserId, Username, PasswordHash, PasswordSalt, FullName, IsActive, CreatedAt, LastLoginAt
            FROM dbo.AdminUsers
            WHERE AdminUserId = @AdminUserId
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@AdminUserId", adminUserId);
        await connection.OpenAsync();
        var list = await ReadListAsync(command);
        return list.FirstOrDefault();
    }

    public async Task<AdminUser?> FindByUsernameAsync(string username)
    {
        const string sql = """
            SELECT AdminUserId, Username, PasswordHash, PasswordSalt, FullName, IsActive, CreatedAt, LastLoginAt
            FROM dbo.AdminUsers
            WHERE Username = @Username
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Username", username.Trim());
        await connection.OpenAsync();
        var list = await ReadListAsync(command);
        return list.FirstOrDefault();
    }

    public async Task<bool> UsernameExistsAsync(string username, int exceptAdminUserId = 0)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM dbo.AdminUsers
            WHERE Username = @Username AND AdminUserId <> @ExceptAdminUserId
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Username", username.Trim());
        command.Parameters.AddWithValue("@ExceptAdminUserId", exceptAdminUserId);
        await connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
    }

    public async Task<int> CreateAsync(AdminUser user)
    {
        const string sql = """
            INSERT INTO dbo.AdminUsers (Username, PasswordHash, PasswordSalt, FullName, IsActive)
            OUTPUT INSERTED.AdminUserId
            VALUES (@Username, @PasswordHash, @PasswordSalt, @FullName, @IsActive)
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, user, includePassword: true);
        await connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task UpdateAsync(AdminUser user, bool updatePassword)
    {
        var sql = updatePassword
            ? """
              UPDATE dbo.AdminUsers
              SET Username = @Username,
                  FullName = @FullName,
                  IsActive = @IsActive,
                  PasswordHash = @PasswordHash,
                  PasswordSalt = @PasswordSalt
              WHERE AdminUserId = @AdminUserId
              """
            : """
              UPDATE dbo.AdminUsers
              SET Username = @Username,
                  FullName = @FullName,
                  IsActive = @IsActive
              WHERE AdminUserId = @AdminUserId
              """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, user, updatePassword);
        command.Parameters.AddWithValue("@AdminUserId", user.AdminUserId);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateLastLoginAsync(int adminUserId)
    {
        const string sql = """
            UPDATE dbo.AdminUsers
            SET LastLoginAt = SYSUTCDATETIME()
            WHERE AdminUserId = @AdminUserId
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@AdminUserId", adminUserId);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<IReadOnlyList<AdminUser>> ReadListAsync(SqlCommand command)
    {
        var result = new List<AdminUser>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new AdminUser
            {
                AdminUserId = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                PasswordSalt = reader.GetString(3),
                FullName = reader.GetString(4),
                IsActive = reader.GetBoolean(5),
                CreatedAt = reader.GetDateTime(6),
                LastLoginAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
            });
        }

        return result;
    }

    private static void AddParameters(SqlCommand command, AdminUser user, bool includePassword)
    {
        command.Parameters.AddWithValue("@Username", user.Username.Trim());
        command.Parameters.AddWithValue("@FullName", user.FullName.Trim());
        command.Parameters.AddWithValue("@IsActive", user.IsActive);

        if (includePassword)
        {
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);
        }
    }
}
