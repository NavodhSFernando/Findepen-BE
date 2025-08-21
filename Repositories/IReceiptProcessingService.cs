using FinDepen_Backend.DTOs;

namespace FinDepen_Backend.Repositories
{
    public interface IReceiptProcessingService
    {
        ReceiptProcessingResponse ProcessReceiptData(AwsReceiptResponse awsResponse);
        TransactionData TransformToTransactionData(ReceiptProcessingResponse receiptData);
        bool ValidateReceiptData(ReceiptProcessingResponse receiptData);
        string GetProcessingStatusMessage(int attempt);
    }
}
