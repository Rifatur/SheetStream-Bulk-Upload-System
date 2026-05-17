namespace SheetStream.DTOs
{
    public class PreviewResult
    {
        public List<ProductUploadDto> ValidRecords { get; set; } = new();
        public List<ValidationError> Errors { get; set; } = new();
        public int TotalRows { get; set; }
        public bool HasErrors => Errors.Any();
    }
}
