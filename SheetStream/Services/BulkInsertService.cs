using Microsoft.EntityFrameworkCore;
using SheetStream.Data;
using SheetStream.DTOs;
using SheetStream.Models;

namespace SheetStream.Services
{
    public class BulkInsertService : IBulkInsertService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BulkInsertService> _logger;
        private readonly IPartyDealerBulkService _partyBulkService;
        public BulkInsertService(AppDbContext context, ILogger<BulkInsertService> logger)
        {
            _context = context;
            _logger = logger;

        }

        public async Task<BulkInsertResult> InsertProductsAsync(List<ProductUploadDto> dtos, CancellationToken ct = default)
        {
            var result = new BulkInsertResult();
            const int batchSize = 1000; // Process in chunks for memory efficiency

            try
            {
                // Get existing SKUs to avoid duplicates
                var incomingSkus = dtos
                    .Where(d => !string.IsNullOrWhiteSpace(d.Sku))
                    .Select(d => d.Sku!.Trim().ToLower()) // ✅ use ToLower, not ToLowerInvariant
                    .Distinct()
                    .ToList();

                List<string> existingSkus = new();

                if (incomingSkus.Count > 0)
                {
                    existingSkus = await _context.Products
                        .Where(p => p.Sku != null && incomingSkus.Contains(p.Sku.Trim().ToLower()))
                        .Select(p => p.Sku!.Trim().ToLower())
                        .ToListAsync(ct);
                }



                var newProducts = dtos
                    .Where(d => string.IsNullOrWhiteSpace(d.Sku) || !existingSkus.Contains(d.Sku.ToLowerInvariant()))
                    .Select(d => new Product
                    {
                        Name = d.Name,
                        Description = d.Description,
                        Price = d.Price,
                        StockQuantity = d.StockQuantity,
                        Category = d.Category,
                        Sku = d.Sku,
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    })
                    .ToList();

                var duplicates = dtos.Count - newProducts.Count;
                if (duplicates > 0)
                    result.Messages.Add($"{duplicates} duplicate SKU(s) skipped");

                // Batch insert using EF Core AddRange (efficient for 10K)
                for (int i = 0; i < newProducts.Count; i += batchSize)
                {
                    var batch = newProducts.Skip(i).Take(batchSize).ToList();
                    await _context.Products.AddRangeAsync(batch, ct);
                    await _context.SaveChangesAsync(ct);

                    // Detach entities to free memory
                    _context.ChangeTracker.Clear();

                    result.InsertedCount += batch.Count;
                    _logger.LogInformation("Inserted batch {BatchNum} of {Total}", i / batchSize + 1, (newProducts.Count + batchSize - 1) / batchSize);
                }

                result.Messages.Add($"Successfully inserted {result.InsertedCount} products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk insert failed");
                result.FailedCount = dtos.Count;
                result.Messages.Add($"Error: {ex.Message}");
            }

            return result;
        }
    }
}
