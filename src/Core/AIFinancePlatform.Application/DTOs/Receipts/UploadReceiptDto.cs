namespace AIFinancePlatform.Application.DTOs.Receipts;

public record UploadReceiptDto(
    string FilePath,
    string OriginalFileName
);
