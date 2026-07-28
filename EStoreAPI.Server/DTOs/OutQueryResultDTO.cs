namespace EStoreAPI.Server.DTOs
{
    public record OutQueryResultDTO
    {
        public List<Dictionary<string, object?>> Rows { get; init; } = new();

        public int RowCount { get; init; }

        // true when the query matched more rows than the cap
        // signals the model to narrow the query instead of assuming it saw everything
        public bool Truncated { get; init; }
    }
}
