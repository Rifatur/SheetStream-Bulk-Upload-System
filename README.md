# SheetStream - ASP.NET Core 8 Bulk Upload System

Bulk insert via Excel/CSV with preview validation. Handles 10,000+ rows smoothly using batched Entity Framework inserts.

## Features

| Feature | Description |
|---------|-------------|
| **Dual Upload** | Separate tabs for Products and Party/Dealers |
| **File Support** | Excel (.xlsx, .xls) and CSV |
| **Preview First** | Validate data before inserting |
| **Auto-Mapping** | Smart column matching for Party/Dealer fields |
| **10K+ Rows** | Batched inserts (1,000 per batch) |
| **Duplicate Check** | Skips existing SKUs / Dealer Codes |
| **Sample Downloads** | Pre-built templates in `/wwwroot/samples/` |

---

## Tech Stack

- **Backend**: ASP.NET Core 8, Entity Framework Core, SQL Server
- **Frontend**: jQuery, Bootstrap 5
