namespace SheetStream.Models
{
    public class PartyDealer
    {
        public int Id { get; set; }

        // Basic Info
        public string Code { get; set; } = string.Empty;          // Unique dealer code
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        // Address
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? Country { get; set; }

        // Business Details
        public string? GstNumber { get; set; }
        public string? PanNumber { get; set; }
        public string? DealerType { get; set; }  // Distributor, Retailer, Wholesaler
        public string? PaymentTerms { get; set; } // Net 30, Net 60, COD

        // Credit
        public decimal? CreditLimit { get; set; }
        public int? CreditDays { get; set; }

        // Status
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
