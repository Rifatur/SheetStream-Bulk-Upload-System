namespace SheetStream.DTOs
{
    public class BulkInsertResult
    {
        public int InsertedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Messages { get; set; } = new();
        public bool Success => FailedCount == 0;
    }
}
