namespace EStoreAPI.Server.DTOs
{
    public class OutQueryResultDTO
    {
        public List<Dictionary<string, object?>> Rows { get; set; } = new();

        public int RowCount { get; set; }

        // true when the query matched more rows than the cap
        // signals the model to narrow the query instead of assuming it saw everything
        public bool Truncated { get; set; }
    }
}
