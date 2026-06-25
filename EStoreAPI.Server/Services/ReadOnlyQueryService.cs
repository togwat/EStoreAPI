using EStoreAPI.Server.DTOs;
using Npgsql;

namespace EStoreAPI.Server.Services
{
    public class ReadOnlyQueryService : IReadOnlyQueryService
    {
        // Hard cap on returned rows so a broad SELECT cannot flood the model's context.
        private const int MaxRows = 100;

        private readonly string _connectionString;

        public ReadOnlyQueryService(IConfiguration configuration)
        {
            // Separate from EF Core's connection
            // Authenticate as the user agent_readonly
            _connectionString = configuration.GetConnectionString("ReadOnlyDatabase")
                ?? throw new InvalidOperationException("ConnectionStrings:ReadOnlyDatabase is not configured.");
        }

        public async Task<OutQueryResultDTO> ExecuteQueryAsync(string sql)
        {
            // plain NpgsqlConnection (pooled per connection string by ADO.NET)
            // so the service can be scoped like every other service
            await using NpgsqlConnection conn = new(_connectionString);
            await conn.OpenAsync();
            await using NpgsqlCommand cmd = new(sql, conn);
            await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

            List<Dictionary<string, object?>> rows = new();
            bool truncated = false;

            // convert rows to dict -> exportable json
            while (await reader.ReadAsync())
            {
                if (rows.Count == MaxRows)
                {
                    truncated = true;
                    break;
                }
                rows.Add(ReadRow(reader));
            }

            return new OutQueryResultDTO
            {
                Rows = rows,
                RowCount = rows.Count,
                Truncated = truncated,
            };
        }

        // Materializes one row as name -> value so any column shape serializes to JSON.
        private static Dictionary<string, object?> ReadRow(NpgsqlDataReader reader)
        {
            Dictionary<string, object?> row = new();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                // indexer (not Add) so duplicate column names (e.g. SELECT j.*, c.*
                // where both have "DeviceId") overwrite instead of throwing
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            return row;
        }
    }
}
