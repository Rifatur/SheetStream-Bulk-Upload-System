using Microsoft.AspNetCore.Mvc;
using SheetStream.DTOs;
using SheetStream.Services;

namespace SheetStream.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFileProcessingService _fileService;
        private readonly IBulkInsertService _bulkService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HomeController> _logger;
        private readonly IPartyDealerBulkService _partyBulkService;
        public HomeController(
            IFileProcessingService fileService,
            IBulkInsertService bulkService,
            IPartyDealerBulkService partyBulkService,
            IWebHostEnvironment env,

        ILogger<HomeController> logger)
        {
            _fileService = fileService;
            _bulkService = bulkService;
            _env = env;
            _partyBulkService = partyBulkService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index() => View();

        // Step 1: Preview file
        [HttpPost]
        public async Task<IActionResult> Preview(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            try
            {
                using var stream = file.OpenReadStream();
                var preview = _fileService.PreviewFile(stream, file.FileName);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Preview failed");
                return BadRequest(new { error = ex.Message });
            }
        }

        // Step 2: Submit validated data
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] List<ProductUploadDto> products)
        {
            if (products == null || products.Count == 0)
                return BadRequest(new { error = "No data to insert" });

            if (products.Count > 15000)
                return BadRequest(new { error = "Maximum 15,000 records allowed per batch" });

            var result = await _bulkService.InsertProductsAsync(products);
            return Ok(result);
        }

        // Download sample files
        [HttpGet]
        public IActionResult DownloadSample(string format = "xlsx")
        {
            var sampleFolder = Path.Combine(_env.WebRootPath, "samples");
            var fileName = format.ToLower() switch
            {
                "csv" => "sample_products.csv",
                "xlsx" => "sample_products.xlsx",
                _ => "sample_products.xlsx"
            };

            var filePath = Path.Combine(sampleFolder, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Sample file not found");

            var mimeType = format.ToLower() switch
            {
                "csv" => "text/csv",
                _ => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            return PhysicalFile(filePath, mimeType, fileName);
        }

        [HttpPost]
        public IActionResult PreviewPartyDealers(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            try
            {
                using var stream = file.OpenReadStream();
                var preview = _fileService.PreviewPartyDealers(stream, file.FileName);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PartyDealer preview failed");
                return BadRequest(new { error = ex.Message });
            }
        }

        // Party/Dealer Submit
        [HttpPost]
        public async Task<IActionResult> SubmitPartyDealers([FromBody] List<PartyDealerUploadDto> dealers)
        {
            if (dealers == null || dealers.Count == 0)
                return BadRequest(new { error = "No data to insert" });

            if (dealers.Count > 15000)
                return BadRequest(new { error = "Maximum 15,000 records allowed" });

            var result = await _partyBulkService.InsertPartyDealersAsync(dealers);
            return Ok(result);
        }

        // Download Party/Dealer Sample
        [HttpGet]
        public IActionResult DownloadPartySample(string format = "xlsx")
        {
            var sampleFolder = Path.Combine(_env.WebRootPath, "samples");
            var fileName = format.ToLower() switch
            {
                "csv" => "sample_party_dealers.csv",
                "xlsx" => "sample_party_dealers.xlsx",
                _ => "sample_party_dealers.xlsx"
            };

            var filePath = Path.Combine(sampleFolder, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound("Sample file not found");

            var mimeType = format.ToLower() switch
            {
                "csv" => "text/csv",
                _ => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            return PhysicalFile(filePath, mimeType, fileName);
        }




    }
}
