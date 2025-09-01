using System.ComponentModel.DataAnnotations;

namespace FinDepen_Backend.DTOs
{
    public class ProcessReceiptRequest
    {
        [Required(ErrorMessage = "Receipt image is required")]
        public string FileBase64 { get; set; } = string.Empty;
    }

    public class ReceiptLineItem
    {
        public string PRODUCT_CODE { get; set; } = string.Empty;
        public string QUANTITY { get; set; } = string.Empty;
        public string UNIT_PRICE { get; set; } = string.Empty;
        public string PRICE { get; set; } = string.Empty;
        public string ITEM { get; set; } = string.Empty;
        public string EXPENSE_ROW { get; set; } = string.Empty;
    }

    public class ReceiptSummary
    {
        public string ADDRESS { get; set; } = string.Empty;
        public string STREET { get; set; } = string.Empty;
        public string CITY { get; set; } = string.Empty;
        public string NAME { get; set; } = string.Empty;
        public string ADDRESS_BLOCK { get; set; } = string.Empty;
        public string AMOUNT_PAID { get; set; } = string.Empty;
        public string INVOICE_RECEIPT_DATE { get; set; } = string.Empty;
        public string INVOICE_RECEIPT_ID { get; set; } = string.Empty;
        public string TOTAL { get; set; } = string.Empty;
        public string VENDOR_ADDRESS { get; set; } = string.Empty;
        public string VENDOR_NAME { get; set; } = string.Empty;
        public string VENDOR_PHONE { get; set; } = string.Empty;
        public string OTHER { get; set; } = string.Empty;
    }

    public class AwsReceiptResponse
    {
        public ReceiptSummary summary { get; set; } = new ReceiptSummary();
        public List<ReceiptLineItem> line_items { get; set; } = new List<ReceiptLineItem>();
    }

    // Legacy DTOs for backward compatibility (keeping the old structure for internal use)
    public class ReceiptItem
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class ReceiptData
    {
        public decimal? Amount { get; set; }
        public string? Date { get; set; }
        public string? Merchant { get; set; }
        public List<ReceiptItem>? Items { get; set; }
        public decimal? Total { get; set; }
        public decimal? Tax { get; set; }
        public string? Category { get; set; }
    }

    public class ReceiptProcessingResponse
    {
        public bool Success { get; set; }
        public ReceiptData? Data { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
    }

    public class TransactionData
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Amount { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string Type { get; set; } = "Expense";
        public string Date { get; set; } = string.Empty;
    }

    public class ReceiptWithTransactionResponse
    {
        public bool Success { get; set; }
        public ReceiptData? Data { get; set; }
        public TransactionData? TransactionData { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
    }
}
