using MAUN.Tomer.Web.Models;
using Microsoft.Data.SqlClient;

namespace MAUN.Tomer.Web.Data;

public class SqlCertificateRepository : ICertificateRepository
{
    private readonly string connectionString;

    public SqlCertificateRepository(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string bulunamadı.");
    }

    public async Task<IReadOnlyList<CertificateInventory>> ListAsync(string? search)
    {
        const string baseSql = """
            SELECT CertificateId, CertificateDate, IdentityOrPassportNo, FullName, CertificateNo, Level,
                   ReadingScore, WritingScore, ListeningScore, SpeakingScore, TotalScore, PassingStatus
            FROM dbo.Tomer_CertificateInventory
            """;

        var sql = baseSql;
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += """

            WHERE IdentityOrPassportNo LIKE @Search OR FullName LIKE @Search OR CertificateNo LIKE @Search
            """;
        }

        sql += " ORDER BY CertificateDate DESC, CertificateId DESC";

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);

        if (!string.IsNullOrWhiteSpace(search))
            command.Parameters.AddWithValue("@Search", $"%{search.Trim()}%");

        await connection.OpenAsync();
        return await ReadListAsync(command);
    }

    public async Task<IReadOnlyList<CertificateInventory>> FindByIdentityAsync(string identityOrPassportNo)
    {
        const string sql = """
            SELECT CertificateId, CertificateDate, IdentityOrPassportNo, FullName, CertificateNo, Level,
                   ReadingScore, WritingScore, ListeningScore, SpeakingScore, TotalScore, PassingStatus
            FROM dbo.Tomer_CertificateInventory
            WHERE IdentityOrPassportNo = @IdentityOrPassportNo
            ORDER BY CertificateDate DESC, CertificateId DESC
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdentityOrPassportNo", identityOrPassportNo.Trim());

        await connection.OpenAsync();
        return await ReadListAsync(command);
    }

    public async Task<CertificateInventory?> GetAsync(int id)
    {
        const string sql = """
            SELECT CertificateId, CertificateDate, IdentityOrPassportNo, FullName, CertificateNo, Level,
                   ReadingScore, WritingScore, ListeningScore, SpeakingScore, TotalScore, PassingStatus
            FROM dbo.Tomer_CertificateInventory
            WHERE CertificateId = @CertificateId
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CertificateId", id);

        await connection.OpenAsync();
        var list = await ReadListAsync(command);
        return list.FirstOrDefault();
    }


    public async Task<bool> HasDuplicateAsync(CertificateInventory certificate)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM dbo.Tomer_CertificateInventory
            WHERE CertificateId <> @CertificateId
              AND (
                    (NULLIF(@CertificateNo, '') IS NOT NULL AND CertificateNo = @CertificateNo)
                    OR (
                        IdentityOrPassportNo = @IdentityOrPassportNo
                        AND CAST(CertificateDate AS date) = CAST(@CertificateDate AS date)
                        AND [Level] = @Level
                    )
                  )
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CertificateId", certificate.CertificateId);
        command.Parameters.AddWithValue("@CertificateNo", certificate.CertificateNo?.Trim() ?? "");
        command.Parameters.AddWithValue("@IdentityOrPassportNo", certificate.IdentityOrPassportNo.Trim());
        command.Parameters.AddWithValue("@CertificateDate", certificate.CertificateDate.Date);
        command.Parameters.AddWithValue("@Level", certificate.Level.Trim());

        await connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
    }
    public async Task<int> CreateAsync(CertificateInventory certificate)
    {
        const string sql = """
            INSERT INTO dbo.Tomer_CertificateInventory
                (CertificateDate, IdentityOrPassportNo, FullName, CertificateNo, Level,
                 ReadingScore, WritingScore, ListeningScore, SpeakingScore, TotalScore, PassingStatus)
            OUTPUT INSERTED.CertificateId
            VALUES
                (@CertificateDate, @IdentityOrPassportNo, @FullName, @CertificateNo, @Level,
                 @ReadingScore, @WritingScore, @ListeningScore, @SpeakingScore, @TotalScore, @PassingStatus)
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, certificate);

        await connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task UpdateAsync(CertificateInventory certificate)
    {
        const string sql = """
            UPDATE dbo.Tomer_CertificateInventory
            SET CertificateDate = @CertificateDate,
                IdentityOrPassportNo = @IdentityOrPassportNo,
                FullName = @FullName,
                CertificateNo = @CertificateNo,
                Level = @Level,
                ReadingScore = @ReadingScore,
                WritingScore = @WritingScore,
                ListeningScore = @ListeningScore,
                SpeakingScore = @SpeakingScore,
                TotalScore = @TotalScore,
                PassingStatus = @PassingStatus
            WHERE CertificateId = @CertificateId
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, certificate);
        command.Parameters.AddWithValue("@CertificateId", certificate.CertificateId);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM dbo.Tomer_CertificateInventory WHERE CertificateId = @CertificateId";

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CertificateId", id);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<IReadOnlyList<CertificateInventory>> ReadListAsync(SqlCommand command)
    {
        var result = new List<CertificateInventory>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new CertificateInventory
            {
                CertificateId = reader.GetInt32(0),
                CertificateDate = reader.GetDateTime(1),
                IdentityOrPassportNo = reader.GetString(2),
                FullName = reader.GetString(3),
                CertificateNo = reader.IsDBNull(4) ? null : reader.GetString(4),
                Level = reader.GetString(5),
                ReadingScore = reader.GetInt32(6),
                WritingScore = reader.GetInt32(7),
                ListeningScore = reader.GetInt32(8),
                SpeakingScore = reader.GetInt32(9),
                TotalScore = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                PassingStatus = reader.IsDBNull(11) ? null : reader.GetString(11)
            });
        }

        return result;
    }

    private static void AddParameters(SqlCommand command, CertificateInventory certificate)
    {
        command.Parameters.AddWithValue("@CertificateDate", certificate.CertificateDate);
        command.Parameters.AddWithValue("@IdentityOrPassportNo", certificate.IdentityOrPassportNo.Trim());
        command.Parameters.AddWithValue("@FullName", certificate.FullName.Trim());
        command.Parameters.AddWithValue("@CertificateNo", string.IsNullOrWhiteSpace(certificate.CertificateNo) ? DBNull.Value : certificate.CertificateNo.Trim());
        command.Parameters.AddWithValue("@Level", certificate.Level.Trim());
        command.Parameters.AddWithValue("@ReadingScore", certificate.ReadingScore);
        command.Parameters.AddWithValue("@WritingScore", certificate.WritingScore);
        command.Parameters.AddWithValue("@ListeningScore", certificate.ListeningScore);
        command.Parameters.AddWithValue("@SpeakingScore", certificate.SpeakingScore);
        command.Parameters.AddWithValue("@TotalScore", certificate.TotalScore.HasValue ? certificate.TotalScore.Value : DBNull.Value);
        command.Parameters.AddWithValue("@PassingStatus", string.IsNullOrWhiteSpace(certificate.PassingStatus) ? DBNull.Value : certificate.PassingStatus.Trim());
    }
}
