using Microsoft.EntityFrameworkCore;
using SheetStream.Data;
using SheetStream.DTOs;
using SheetStream.Models;

namespace SheetStream.Services
{
    public class PartyDealerBulkService : IPartyDealerBulkService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PartyDealerBulkService> _logger;

        public PartyDealerBulkService(AppDbContext context, ILogger<PartyDealerBulkService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PartyDealerBulkResult> InsertPartyDealersAsync(List<PartyDealerUploadDto> dtos, CancellationToken ct = default)
        {
            var result = new PartyDealerBulkResult();
            const int batchSize = 1000;

            try
            {
                // Check existing codes, GSTs, emails
                var incomingCodes = dtos
                    .Where(d => !string.IsNullOrWhiteSpace(d.Code))
                    .Select(d => d.Code!.Trim().ToLower())
                    .Distinct()
                    .ToList();

                var incomingGsts = dtos
                    .Where(d => !string.IsNullOrWhiteSpace(d.GstNumber))
                    .Select(d => d.GstNumber!.Trim().ToLower())
                    .Distinct()
                    .ToList();

                var incomingEmails = dtos
                    .Where(d => !string.IsNullOrWhiteSpace(d.Email))
                    .Select(d => d.Email!.Trim().ToLower())
                    .Distinct()
                    .ToList();

                List<string> existingCodes = new();
                List<string> existingGsts = new();
                List<string> existingEmails = new();

                if (incomingCodes.Count > 0)
                {
                    existingCodes = await _context.PartyDealers
                        .Where(p => p.Code != null && incomingCodes.Contains(p.Code.Trim().ToLower()))
                        .Select(p => p.Code!.Trim().ToLower())
                        .ToListAsync(ct);
                }

                if (incomingGsts.Count > 0)
                {
                    existingGsts = await _context.PartyDealers
                        .Where(p => p.GstNumber != null && incomingGsts.Contains(p.GstNumber.Trim().ToLower()))
                        .Select(p => p.GstNumber!.Trim().ToLower())
                        .ToListAsync(ct);
                }

                if (incomingEmails.Count > 0)
                {
                    existingEmails = await _context.PartyDealers
                        .Where(p => p.Email != null && incomingEmails.Contains(p.Email.Trim().ToLower()))
                        .Select(p => p.Email!.Trim().ToLower())
                        .ToListAsync(ct);
                }


                var newDealers = dtos
                    .Where(d => !existingCodes.Contains(d.Code.ToLowerInvariant()))
                    .Select(d => new PartyDealer
                    {
                        Code = d.Code,
                        Name = d.Name,
                        ContactPerson = d.ContactPerson,
                        Phone = d.Phone,
                        Email = d.Email,
                        Address = d.Address,
                        City = d.City,
                        State = d.State,
                        Pincode = d.Pincode,
                        Country = d.Country,
                        GstNumber = d.GstNumber,
                        PanNumber = d.PanNumber,
                        DealerType = d.DealerType,
                        PaymentTerms = d.PaymentTerms,
                        CreditLimit = d.CreditLimit,
                        CreditDays = d.CreditDays,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                result.SkippedCount = dtos.Count - newDealers.Count;
                if (result.SkippedCount > 0)
                    result.Messages.Add($"{result.SkippedCount} duplicate code(s) skipped");

                // Batch insert
                for (int i = 0; i < newDealers.Count; i += batchSize)
                {
                    var batch = newDealers.Skip(i).Take(batchSize).ToList();
                    await _context.PartyDealers.AddRangeAsync(batch, ct);
                    await _context.SaveChangesAsync(ct);
                    _context.ChangeTracker.Clear();

                    result.InsertedCount += batch.Count;
                    _logger.LogInformation("PartyDealer batch {Batch} inserted", i / batchSize + 1);
                }

                result.Messages.Add($"Successfully inserted {result.InsertedCount} dealers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PartyDealer bulk insert failed");
                result.Messages.Add($"Error: {ex.Message}");
            }

            return result;
        }


    }
}
