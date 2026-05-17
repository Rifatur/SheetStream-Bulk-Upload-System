using SheetStream.DTOs;

namespace SheetStream.Services
{
    public interface IPartyDealerBulkService
    {
        Task<PartyDealerBulkResult> InsertPartyDealersAsync(List<PartyDealerUploadDto> dealers, CancellationToken ct = default);
    }
}
