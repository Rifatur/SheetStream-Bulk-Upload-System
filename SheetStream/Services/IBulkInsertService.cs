using SheetStream.DTOs;

namespace SheetStream.Services
{
    public interface IBulkInsertService
    {
        Task<BulkInsertResult> InsertProductsAsync(List<ProductUploadDto> products, CancellationToken ct = default);
    }
}
