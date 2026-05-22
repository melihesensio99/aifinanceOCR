using System;

namespace AIFinancePlatform.Application.Common.Interfaces.Events;

public record ReceiptUploadedEvent(
    Guid UserId,
    string ImagePath,
    string OriginalFileName
);
