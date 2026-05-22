namespace AIFinancePlatform.Application.CQRS.Commands.Receipts.UploadReceipt;

public record UploadReceiptCommandResult(
    string FilePath,
    string OriginalFileName,
    bool IsSuccess,
    string Message
);
