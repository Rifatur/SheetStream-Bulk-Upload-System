using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;
using SheetStream.DTOs;
using System.Globalization;

namespace SheetStream.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        public PreviewResult PreviewFile(Stream fileStream, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".csv" => PreviewCsv(fileStream),
                ".xlsx" or ".xls" => PreviewExcel(fileStream),
                _ => throw new NotSupportedException($"File type {extension} not supported")
            };
        }

        private PreviewResult PreviewCsv(Stream stream)
        {
            var result = new PreviewResult();

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var records = csv.GetRecords<ProductUploadDto>();
            int rowNum = 1;

            foreach (var record in records)
            {
                rowNum++;
                var errors = ValidateRecord(record, rowNum);

                if (errors.Any())
                {
                    result.Errors.AddRange(errors);
                }
                else
                {
                    result.ValidRecords.Add(record);
                }
            }

            result.TotalRows = rowNum - 1;
            return result;
        }

        private PreviewResult PreviewExcel(Stream stream)
        {
            var result = new PreviewResult();
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount < 2) return result; // No data rows

            // Read headers from row 1
            var headers = new Dictionary<string, int>();
            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                var header = worksheet.Cells[1, col].Text?.Trim();
                if (!string.IsNullOrEmpty(header))
                    headers[header.ToLowerInvariant()] = col;
            }

            for (int row = 2; row <= rowCount; row++)
            {
                var dto = new ProductUploadDto
                {
                    Name = GetCellValue(worksheet, headers, row, "name"),
                    Description = GetCellValue(worksheet, headers, row, "description"),
                    Sku = GetCellValue(worksheet, headers, row, "sku"),
                    Category = GetCellValue(worksheet, headers, row, "category"),
                    Price = ParseDecimal(GetCellValue(worksheet, headers, row, "price")),
                    StockQuantity = ParseInt(GetCellValue(worksheet, headers, row, "stockquantity"))
                };

                var errors = ValidateRecord(dto, row);
                if (errors.Any())
                    result.Errors.AddRange(errors);
                else
                    result.ValidRecords.Add(dto);
            }

            result.TotalRows = rowCount - 1;
            return result;
        }

        private string? GetCellValue(ExcelWorksheet sheet, Dictionary<string, int> headers, int row, string columnName)
        {
            if (headers.TryGetValue(columnName.ToLowerInvariant(), out var col))
                return sheet.Cells[row, col].Text?.Trim();
            return null;
        }

        private decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
            return result;
        }

        private int ParseInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            int.TryParse(value, out var result);
            return result;
        }

        private List<ValidationError> ValidateRecord(ProductUploadDto record, int rowNum)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(record.Name))
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "Name", Message = "Name is required" });

            if (record.Price <= 0)
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "Price", Message = "Price must be greater than 0" });

            if (record.StockQuantity < 0)
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "StockQuantity", Message = "Stock cannot be negative" });

            if (!string.IsNullOrEmpty(record.Sku) && record.Sku.Length > 50)
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "Sku", Message = "SKU too long (max 50)" });

            return errors;
        }

        public PartyDealerPreviewResult PreviewPartyDealers(Stream stream, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".csv" => PreviewPartyCsv(stream),
                ".xlsx" or ".xls" => PreviewPartyExcel(stream),
                _ => throw new NotSupportedException($"File type {extension} not supported")
            };
        }

        private PartyDealerPreviewResult PreviewPartyCsv(Stream stream)
        {
            var result = new PartyDealerPreviewResult();

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var records = csv.GetRecords<PartyDealerUploadDto>();
            int rowNum = 1;

            foreach (var record in records)
            {
                rowNum++;
                var errors = ValidatePartyRecord(record, rowNum);

                if (errors.Any())
                    result.Errors.AddRange(errors);
                else
                    result.ValidRecords.Add(record);
            }

            result.TotalRows = rowNum - 1;
            return result;
        }

        private PartyDealerPreviewResult PreviewPartyExcel(Stream stream)
        {
            var result = new PartyDealerPreviewResult();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount < 2) return result;

            var headers = new Dictionary<string, int>();
            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                var header = worksheet.Cells[1, col].Text?.Trim();
                if (!string.IsNullOrEmpty(header))
                    headers[header.ToLowerInvariant().Replace(" ", "")] = col;
            }

            for (int row = 2; row <= rowCount; row++)
            {
                var dto = new PartyDealerUploadDto
                {
                    Code = GetPartyCellValue(worksheet, headers, row, "code"),
                    Name = GetPartyCellValue(worksheet, headers, row, "name"),
                    ContactPerson = GetPartyCellValue(worksheet, headers, row, "contactperson"),
                    Phone = GetPartyCellValue(worksheet, headers, row, "phone"),
                    Email = GetPartyCellValue(worksheet, headers, row, "email"),
                    Address = GetPartyCellValue(worksheet, headers, row, "address"),
                    City = GetPartyCellValue(worksheet, headers, row, "city"),
                    State = GetPartyCellValue(worksheet, headers, row, "state"),
                    Pincode = GetPartyCellValue(worksheet, headers, row, "pincode"),
                    Country = GetPartyCellValue(worksheet, headers, row, "country"),
                    GstNumber = GetPartyCellValue(worksheet, headers, row, "gstnumber"),
                    PanNumber = GetPartyCellValue(worksheet, headers, row, "pannumber"),
                    DealerType = GetPartyCellValue(worksheet, headers, row, "dealertype"),
                    PaymentTerms = GetPartyCellValue(worksheet, headers, row, "paymentterms"),
                    CreditLimit = ParseNullableDecimal(GetPartyCellValue(worksheet, headers, row, "creditlimit")),
                    CreditDays = ParseNullableInt(GetPartyCellValue(worksheet, headers, row, "creditdays"))
                };

                var errors = ValidatePartyRecord(dto, row);
                if (errors.Any())
                    result.Errors.AddRange(errors);
                else
                    result.ValidRecords.Add(dto);
            }

            result.TotalRows = rowCount - 1;
            return result;
        }

        private string? GetPartyCellValue(ExcelWorksheet sheet, Dictionary<string, int> headers, int row, string columnName)
        {
            if (headers.TryGetValue(columnName.ToLowerInvariant(), out var col))
                return sheet.Cells[row, col].Text?.Trim();
            return null;
        }

        private decimal? ParseNullableDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
        }

        private int? ParseNullableInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return int.TryParse(value, out var result) ? result : null;
        }

        private List<ValidationError> ValidatePartyRecord(PartyDealerUploadDto record, int rowNum)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(record.Code))
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "Code", Message = "Code is required" });
            else if (record.Code.Length > 20)
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "Code", Message = "Code max 20 characters" });

            if (string.IsNullOrWhiteSpace(record.Name))
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "Name", Message = "Name is required" });

            if (!string.IsNullOrEmpty(record.Email) && !IsValidEmail(record.Email))
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "Email", Message = "Invalid email format" });

            if (!string.IsNullOrEmpty(record.Phone) && record.Phone.Length > 20)
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "Phone", Message = "Phone too long" });

            if (!string.IsNullOrEmpty(record.GstNumber) && record.GstNumber.Length != 15)
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "GST", Message = "GST must be 15 characters" });

            if (record.CreditLimit.HasValue && record.CreditLimit.Value < 0)
                errors.Add(new ValidationError { RowNumber = rowNum, Field = "CreditLimit", Message = "Cannot be negative" });

            return errors;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }




    }
}
