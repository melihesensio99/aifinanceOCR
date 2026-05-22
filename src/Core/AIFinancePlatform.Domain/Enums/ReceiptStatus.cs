namespace AIFinancePlatform.Domain.Enums;

public enum ReceiptStatus
{
    Uploaded = 1,
    ProcessingOcr = 2,
    ProcessingAi = 3,
    Completed = 4,
    Failed = 5
}
