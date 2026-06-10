using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EStoreAPI.Server.Data
{
    // Applies the table allowlist for the agent's read-only SQL query tool.
    // Runs at startup, after EF migrations, connected as the schema owner.
    public static class AgentReadOnlyGrants
    {
        // Tables the read-only agent role may SELECT from
        private static readonly string[] ReadableTables =
        {
            "Customers",
            "Devices",
            "Problems",
            "Jobs",
            "JobProblems",
        };

        public static void Apply(EStoreDbContext db, IConfiguration configuration, ILogger logger)
        {
            string? connectionString = configuration.GetConnectionString("ReadOnlyDatabase");
            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogWarning("ConnectionStrings:ReadOnlyDatabase is not configured; skipping agent read-only grants.");
                return;
            }

            // the role name is taken from the read-only connection string so the two can never drift apart
            string? role = new NpgsqlConnectionStringBuilder(connectionString).Username;
            if (string.IsNullOrEmpty(role))
            {
                logger.LogWarning("ReadOnlyDatabase connection string has no Username; skipping agent read-only grants.");
                return;
            }

            try
            {
                db.Database.ExecuteSqlRaw(BuildGrantSql(role));
                logger.LogInformation("Applied read-only grants for role {Role} on: {Tables}", role, string.Join(", ", ReadableTables));
            }
            catch (PostgresException ex)
            {
                // most likely the role doesn't exist yet (init script not run on this database)
                // the API can still serve, only the SQL tool breaks
                logger.LogWarning(ex, "Could not apply read-only grants for role {Role}; the agent SQL tool will not work. Run db/init/01-agent-readonly.sh against this database.", role);
            }
        }

        private static string BuildGrantSql(string role)
        {
            string quotedRole = QuoteIdentifier(role);
            string quotedTables = string.Join(", ", ReadableTables.Select(QuoteIdentifier));

            // Revoke all prvileges and regrant them, so only the allowlist is guaranteed readable
            return $"""
                REVOKE SELECT ON ALL TABLES IN SCHEMA public FROM {quotedRole};
                ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE SELECT ON TABLES FROM {quotedRole};
                GRANT SELECT ON {quotedTables} TO {quotedRole};
                """;
        }

        // role/table names cannot be SQL parameters, so they are escaped as double-quoted identifiers instead
        private static string QuoteIdentifier(string name) => '"' + name.Replace("\"", "\"\"") + '"';
    }
}