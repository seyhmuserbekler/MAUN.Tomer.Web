using Microsoft.Data.SqlClient;

namespace MAUN.Tomer.Web.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string bulunamadı.");

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("Connection string içinde veritabanı adı bulunamadı.");

        builder.InitialCatalog = "master";
        await EnsureDatabaseAsync(builder.ConnectionString, databaseName);
        await EnsureSchemaAsync(connectionString);
        await EnsureDefaultAdminAsync(connectionString);
    }

    private static async Task EnsureDatabaseAsync(string masterConnectionString, string databaseName)
    {
        var sql = $"""
            IF DB_ID(N'{EscapeSql(databaseName)}') IS NULL
            BEGIN
                CREATE DATABASE [{EscapeIdentifier(databaseName)}];
            END
            """;

        await using var connection = new SqlConnection(masterConnectionString);
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static async Task EnsureSchemaAsync(string connectionString)
    {
        const string sql = """
            IF OBJECT_ID(N'dbo.Tomer_CertificateInventory', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Tomer_CertificateInventory
                (
                    CertificateId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tomer_CertificateInventory PRIMARY KEY,
                    CertificateDate DATETIME NOT NULL,
                    IdentityOrPassportNo NVARCHAR(50) NOT NULL,
                    FullName NVARCHAR(255) NOT NULL,
                    CertificateNo NVARCHAR(100) NULL,
                    [Level] NVARCHAR(50) NOT NULL,
                    ReadingScore INT NOT NULL,
                    WritingScore INT NOT NULL,
                    ListeningScore INT NOT NULL,
                    SpeakingScore INT NOT NULL,
                    TotalScore INT NULL,
                    PassingStatus NVARCHAR(100) NULL
                );

                CREATE INDEX IX_Tomer_CertificateInventory_IdentityOrPassportNo
                    ON dbo.Tomer_CertificateInventory(IdentityOrPassportNo);
            END;

            IF OBJECT_ID(N'dbo.AdminUsers', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.AdminUsers
                (
                    AdminUserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AdminUsers PRIMARY KEY,
                    Username NVARCHAR(50) NOT NULL,
                    PasswordHash NVARCHAR(200) NOT NULL,
                    PasswordSalt NVARCHAR(200) NOT NULL,
                    FullName NVARCHAR(150) NOT NULL,
                    IsActive BIT NOT NULL CONSTRAINT DF_AdminUsers_IsActive DEFAULT(1),
                    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AdminUsers_CreatedAt DEFAULT(SYSUTCDATETIME()),
                    LastLoginAt DATETIME2 NULL
                );

                CREATE UNIQUE INDEX UX_AdminUsers_Username ON dbo.AdminUsers(Username);
            END;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static async Task EnsureDefaultAdminAsync(string connectionString)
    {
        const string existsSql = "SELECT COUNT(1) FROM dbo.AdminUsers WHERE Username = @Username";
        const string insertSql = """
            INSERT INTO dbo.AdminUsers (Username, PasswordHash, PasswordSalt, FullName, IsActive)
            VALUES (@Username, @PasswordHash, @PasswordSalt, @FullName, 1)
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using (var existsCommand = new SqlCommand(existsSql, connection))
        {
            existsCommand.Parameters.AddWithValue("@Username", "admin");
            if (Convert.ToInt32(await existsCommand.ExecuteScalarAsync()) > 0)
                return;
        }

        var password = PasswordHasher.HashPassword("maun2026");
        await using var insertCommand = new SqlCommand(insertSql, connection);
        insertCommand.Parameters.AddWithValue("@Username", "admin");
        insertCommand.Parameters.AddWithValue("@PasswordHash", password.Hash);
        insertCommand.Parameters.AddWithValue("@PasswordSalt", password.Salt);
        insertCommand.Parameters.AddWithValue("@FullName", "Sistem Yöneticisi");
        await insertCommand.ExecuteNonQueryAsync();
    }

    private static string EscapeSql(string value)
    {
        return value.Replace("'", "''");
    }

    private static string EscapeIdentifier(string value)
    {
        return value.Replace("]", "]]");
    }
}
