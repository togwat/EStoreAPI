using ModelContextProtocol.Server;
using EStoreAPI.Server.DTOs;
using EStoreAPI.Server.Services;
using System.ComponentModel;
using Npgsql;

[McpServerToolType]
public class QueryTools
{
    private readonly IReadOnlyQueryService _service;

    public QueryTools(IReadOnlyQueryService service)
    {
        _service = service;
    }

    [McpServerTool, Description("""
        Run a read-only SQL SELECT query directly against the store's PostgreSQL database. Use this only when the dedicated tools cannot answer the question.
        At most 200 rows are returned, so if the result says truncated, then the returned data is incomplete; refine the query instead of assuming you saw all rows.

        Schema (identifiers are PascalCase and MUST be double-quoted, e.g. SELECT "JobId" FROM "Jobs"):
        - "Customers": "CustomerId" int PK, "CustomerName" text NULL, "PhoneNumber" text, "PhoneNumberSecondary" text NULL, "Email" text NULL, "Address" text NULL
        - "Devices": "DeviceId" int PK, "DeviceName" text, "ModelNumber" text NULL, "DeviceType" text
        - "Problems": "ProblemId" int PK, "ProblemName" text, "DeviceId" int FK -> Devices, "Price" numeric, "LabourPrice" numeric
        - "Jobs": "JobId" int PK, "CustomerId" int FK -> Customers, "DeviceId" int FK -> Devices, "ReceiveTime" timestamptz, "PickupTime" timestamptz NULL, "EstimatedPickupTime" timestamptz NULL, "Note" text NULL, "EstimatedPrice" numeric NULL, "CollectedPrice" numeric NULL, "IsFinished" boolean, "WarrantyOfJobId" int FK -> Jobs
        - "JobProblems": join table, "JobId" int FK -> Jobs, "ProblemId" int FK -> Problems
        """)]
    public async Task<OutQueryResultDTO> QueryDatabaseAsync(
        [Description("A single PostgreSQL SELECT statement. Double-quote all identifiers.")] string sql)
    {
        try
        {
            return await _service.ExecuteQueryAsync(sql);
        }
        catch (PostgresException ex)
        {
            // surface the Postgres error so the model can correct its SQL and retry
            throw new Exception($"Query failed: {ex.MessageText} (SQLSTATE {ex.SqlState})");
        }
    }
}
