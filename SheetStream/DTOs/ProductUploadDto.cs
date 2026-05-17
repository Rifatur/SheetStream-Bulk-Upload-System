namespace SheetStream.DTOs
{
    public class ProductUploadDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Category { get; set; }
        public string? Sku { get; set; }
    }

}
