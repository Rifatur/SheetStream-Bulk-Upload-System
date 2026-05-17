namespace SheetStream.DTOs
{
    public class PartyDealerPreviewResult
    {
        public List<PartyDealerUploadDto> ValidRecords { get; set; } = new();
        public List<ValidationError> Errors { get; set; } = new();
        public int TotalRows { get; set; }
        public bool HasErrors => Errors.Any();
    }
    public class PartyDealerBulkResult
    {
        public int InsertedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Messages { get; set; } = new();
        public bool Success => SkippedCount == 0 || InsertedCount > 0;
    }

}
