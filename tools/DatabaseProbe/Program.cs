using System.Text.Json;
using MySqlConnector;

// Read-only preflight. Run from the repository root. Never prints connection strings or row contents.
var configured = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
if (string.IsNullOrWhiteSpace(configured))
{
    using var config = JsonDocument.Parse(await File.ReadAllTextAsync("backend/MiniBrasfoot.Api/appsettings.Development.json"));
    configured = config.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString();
}
var connectionOptions = new MySqlConnectionStringBuilder(configured
    ?? throw new InvalidOperationException("Configure the existing MySQL connection.")) { ConnectionTimeout = 5 };
await using var connection = new MySqlConnection(connectionOptions.ConnectionString);
try
{
    await connection.OpenAsync();
    var version = await Scalar("SELECT VERSION()");
    var database = await Scalar("SELECT DATABASE()");
    var teamCount = await Scalar("SELECT COUNT(*) FROM Teams");
    var playerCount = await Scalar("SELECT COUNT(*) FROM Players");
    var orphanCount = await Scalar("SELECT COUNT(*) FROM Players p LEFT JOIN Teams t ON t.Id = p.TeamId WHERE t.Id IS NULL");
    var historyExists = Convert.ToInt32(await Scalar("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = '__EFMigrationsHistory'")) > 0;
    var migrations = new List<string>();
    if (historyExists)
    {
        await using var query = new MySqlCommand("SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId", connection);
        await using var reader = await query.ExecuteReaderAsync();
        while (await reader.ReadAsync()) migrations.Add(reader.GetString(0));
    }
    var foreignKeys = await Scalar("SELECT COUNT(*) FROM information_schema.referential_constraints WHERE constraint_schema = DATABASE() AND table_name = 'Players' AND referenced_table_name = 'Teams'");
    Console.WriteLine(JsonSerializer.Serialize(new { version, database, teamCount, playerCount, orphanCount, historyExists, migrations, foreignKeys },
        new JsonSerializerOptions { WriteIndented = true }));
}
catch (MySqlException ex)
{
    Console.Error.WriteLine($"MySQL preflight failed (code {ex.Number}). Check server availability and local credentials. No changes were made.");
    return 1;
}
return 0;

async Task<object?> Scalar(string sql)
{
    await using var query = new MySqlCommand(sql, connection) { CommandTimeout = 10 };
    return await query.ExecuteScalarAsync();
}
