using FinDepen_Backend.DTOs;
using FinDepen_Backend.Repositories;
using FinDepen_Backend.Constants;
using Microsoft.Extensions.Logging;

namespace FinDepen_Backend.Services
{
    public class ReceiptProcessingService : IReceiptProcessingService
    {
        private readonly ILogger<ReceiptProcessingService> _logger;

        public ReceiptProcessingService(ILogger<ReceiptProcessingService> logger)
        {
            _logger = logger;
        }

        public ReceiptProcessingResponse ProcessReceiptData(AwsReceiptResponse awsResponse)
        {
            try
            {
                if (awsResponse == null)
                {
                    _logger.LogWarning("Null AWS response received");
                    return new ReceiptProcessingResponse
                    {
                        Success = false,
                        Error = "Invalid response from receipt processing service"
                    };
                }

                // Transform AWS response to our internal format
                var receiptData = TransformAwsResponseToReceiptData(awsResponse);
                
                var receiptResponse = new ReceiptProcessingResponse
                {
                    Success = true,
                    Data = receiptData,
                    Error = null,
                    Message = "Receipt processed successfully"
                };

                // Validate the data
                if (!ValidateReceiptData(receiptResponse))
                {
                    _logger.LogWarning("Receipt data validation failed after processing");
                    return new ReceiptProcessingResponse
                    {
                        Success = false,
                        Error = "Invalid receipt data received from processing service"
                    };
                }

                if (receiptResponse.Success)
                {
                    _logger.LogInformation("Receipt data processed successfully. Vendor: {Vendor}, Amount: {Amount}", 
                        receiptResponse.Data?.Merchant, receiptResponse.Data?.Total ?? receiptResponse.Data?.Amount);
                }
                else
                {
                    _logger.LogWarning("Receipt processing failed. Error: {Error}", receiptResponse.Error);
                }

                return receiptResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing receipt data");
                return new ReceiptProcessingResponse
                {
                    Success = false,
                    Error = "An error occurred while processing receipt data"
                };
            }
        }

        private ReceiptData TransformAwsResponseToReceiptData(AwsReceiptResponse awsResponse)
        {
            var summary = awsResponse.summary;
            var lineItems = awsResponse.line_items;

            // Parse amount from string to decimal
            decimal? amount = null;
            decimal? total = null;
            
            if (decimal.TryParse(summary.AMOUNT_PAID, out var amountPaid))
            {
                amount = amountPaid;
            }
            
            if (decimal.TryParse(summary.TOTAL, out var totalAmount))
            {
                total = totalAmount;
            }

            // Transform line items to our format
            var items = new List<ReceiptItem>();
            foreach (var lineItem in lineItems)
            {
                if (decimal.TryParse(lineItem.PRICE, out var price) && 
                    int.TryParse(lineItem.QUANTITY, out var quantity))
                {
                    items.Add(new ReceiptItem
                    {
                        Name = lineItem.ITEM,
                        Price = price,
                        Quantity = quantity
                    });
                }
            }

            return new ReceiptData
            {
                Amount = amount,
                Total = total,
                Date = summary.INVOICE_RECEIPT_DATE,
                Merchant = summary.VENDOR_NAME,
                Items = items,
                Category = DetermineCategoryFromVendor(summary.VENDOR_NAME)
            };
        }

        public TransactionData TransformToTransactionData(ReceiptProcessingResponse receiptData)
        {
            if (!receiptData.Success || receiptData.Data == null)
            {
                _logger.LogWarning("Cannot transform receipt data: Success={Success}, HasData={HasData}", 
                    receiptData.Success, receiptData.Data != null);
                return null;
            }

            var data = receiptData.Data;
            
            // Create title from vendor name
            string title = data.Merchant ?? "Receipt Purchase";
            
            // Add item count to title if there are multiple items
            if (data.Items != null && data.Items.Count > 1)
            {
                title = $"{data.Merchant ?? "Purchase"} ({data.Items.Count} items)";
            }

            // Create description from items if available
            string description = null;
            if (data.Items != null && data.Items.Count > 0)
            {
                var itemDescriptions = data.Items.Select(item => 
                    $"{item.Name} - ${item.Price:F2}"
                );
                description = string.Join(", ", itemDescriptions);
            }

            // Use total amount or fallback to amount
            var amount = (data.Total ?? data.Amount ?? 0).ToString("F2");

            // Use the category from the data
            string category = data.Category ?? "Miscellaneous";

            // Format date
            string date = FormatDate(data.Date);

            var transactionData = new TransactionData
            {
                Title = title,
                Description = description,
                Amount = amount,
                Category = category,
                Type = "Expense", // Receipts are typically expenses
                Date = date
            };

            _logger.LogInformation("Transformed transaction data: {Title}, {Amount}, {Category}", 
                transactionData.Title, transactionData.Amount, transactionData.Category);

            return transactionData;
        }

        public bool ValidateReceiptData(ReceiptProcessingResponse receiptData)
        {
            if (!receiptData.Success || receiptData.Data == null)
            {
                _logger.LogWarning("Receipt data validation failed: Success={Success}, HasData={HasData}", 
                    receiptData.Success, receiptData.Data != null);
                return false;
            }

            var data = receiptData.Data;
            
            // Check if we have at least an amount or total
            if (!data.Amount.HasValue && !data.Total.HasValue)
            {
                _logger.LogWarning("No amount found in receipt data");
                return false;
            }

            // Check if amount/total is valid
            var amount = data.Total ?? data.Amount;
            if (!amount.HasValue || amount <= 0)
            {
                _logger.LogWarning("Invalid amount in receipt data: {Amount}", amount);
                return false;
            }

            _logger.LogInformation("Receipt data validation passed");
            return true;
        }

        public string GetProcessingStatusMessage(int attempt)
        {
            if (attempt == 0)
            {
                return "Processing Receipt...";
            }
            else
            {
                return $"Processing Receipt... (Attempt {attempt + 1})";
            }
        }

        private string DetermineCategoryFromVendor(string vendorName)
        {
            if (string.IsNullOrEmpty(vendorName))
            {
                return "Miscellaneous";
            }

            var vendorLower = vendorName.ToLower();
            
            // Food category
            if (vendorLower.Contains("restaurant") || vendorLower.Contains("food") ||
                vendorLower.Contains("kfc") || vendorLower.Contains("hotel") ||
                vendorLower.Contains("pizza") || vendorLower.Contains("burger") ||
                vendorLower.Contains("cafe") || vendorLower.Contains("dine") ||
                vendorLower.Contains("coffee") || vendorLower.Contains("tea") ||
                vendorLower.Contains("hotel") || vendorLower.Contains("caterer"))
            {
                return "Food";
            }
            
            // Grocery category
            else if (vendorLower.Contains("grocery") || vendorLower.Contains("supermarket") || 
                vendorLower.Contains("spar") || vendorLower.Contains("keells") ||
                vendorLower.Contains("cargills") || vendorLower.Contains("glowmark") ||
                vendorLower.Contains("mart") || vendorLower.Contains("stores"))
            {
                return "Grocery";
            }
            
            // Transportation category
            else if (vendorLower.Contains("ceypetco") || vendorLower.Contains("ioc") ||
                vendorLower.Contains("shell") || vendorLower.Contains("gas") ||
                vendorLower.Contains("transport") || vendorLower.Contains("travel") ||
                vendorLower.Contains("uber") || vendorLower.Contains("taxi") ||
                vendorLower.Contains("bus") || vendorLower.Contains("train") ||
                vendorLower.Contains("tour") || vendorLower.Contains("coach") ||
                vendorLower.Contains("cab") || vendorLower.Contains("ride"))
            {
                return "Transportation";
            }
            
            // Health category
            else if (vendorLower.Contains("pharmacy") || vendorLower.Contains("medical") ||
                vendorLower.Contains("pharma") || vendorLower.Contains("medi") ||
                vendorLower.Contains("drug") || vendorLower.Contains("hospital") ||
                vendorLower.Contains("clinic") || vendorLower.Contains("doctor") ||
                vendorLower.Contains("dispensary") || vendorLower.Contains("pharmaceutical")) 
            {
                return "Health";
            }
            
            // Entertainment category
            else if (vendorLower.Contains("entertainment") || vendorLower.Contains("movie") ||
                vendorLower.Contains("netflix") || vendorLower.Contains("spotify") ||
                vendorLower.Contains("theater") || vendorLower.Contains("cinema") ||
                vendorLower.Contains("game"))
            {
                return "Entertainment";
            }
            
            // Education category
            else if (vendorLower.Contains("book") || vendorLower.Contains("printing") ||
                vendorLower.Contains("school") || vendorLower.Contains("university") ||
                vendorLower.Contains("college") || vendorLower.Contains("library") ||
                vendorLower.Contains("institute") || vendorLower.Contains("udemy") ||
                vendorLower.Contains("coursera") || vendorLower.Contains("board") ||
                vendorLower.Contains("campus") || vendorLower.Contains("graphics"))
            {
                return "Education";
            }
            
            // Rent category (for property-related vendors)
            else if (vendorLower.Contains("rent") || vendorLower.Contains("apartment") ||
                vendorLower.Contains("housing") || vendorLower.Contains("property") ||
                vendorLower.Contains("real estate") || vendorLower.Contains("landlord"))
            {
                return "Rent";
            }

            // Default to Miscellaneous for unknown vendors
            return "Miscellaneous";
        }

        private string FormatDate(string receiptDate)
        {
            // Default to today
            var date = DateTime.Today;

            if (!string.IsNullOrEmpty(receiptDate))
            {
                try
                {
                    // Try to parse the date in various formats
                    if (DateTime.TryParse(receiptDate, out var parsedDate))
                    {
                        date = parsedDate;
                    }
                    else if (DateTime.TryParseExact(receiptDate, "M/d/yyyy", null, System.Globalization.DateTimeStyles.None, out var exactDate))
                    {
                        date = exactDate;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse receipt date: {Date}", receiptDate);
                }
            }

            return date.ToString("yyyy-MM-dd");
        }
    }
}
