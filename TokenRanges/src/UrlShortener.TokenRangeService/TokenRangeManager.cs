using Npgsql;

namespace UrlShortener.TokenRangeService;

public class TokenRangeManager
{
    private readonly string _connectionString;

    private const string SqlQuery = $"""
                                      INSERT INTO "TokenRanges" ("MachineIdentifier", "Start", "End")
                                     VALUES (@MachineIdentifier,
                                     COALESCE((SELECT MAX("End") FROM "TokenRanges") + 1, 1000),
                                     COALESCE((SELECT MAX("End") FROM "TokenRanges") + 1000, 2000)
                                     )
                                     RETURNING "Id", "MachineIdentifier", "Start","End";
                                     """;

    public TokenRangeManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<TokenRangeResponse> AssignRangeAsync(string machineIdentifier)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(SqlQuery, connection);
        command.Parameters.AddWithValue("@MachineIdentifier", machineIdentifier);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new TokenRangeResponse(
                reader.GetInt64(2),
                reader.GetInt64(3)
            );
        }

        throw new Exception("Failed to assign token range.");
    }
}

public class FailedToAssignTokenRangeException(string message) : Exception(message);