using SheetStream.DTOs;

namespace SheetStream.Services
{
    public interface IFileProcessingService
    {
        PreviewResult PreviewFile(Stream fileStream, string fileName);
        PartyDealerPreviewResult PreviewPartyDealers(Stream fileStream, string fileName);
    }
}
