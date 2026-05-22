using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AIFinancePlatform.Application.Common.Interfaces.Events;
using AIFinancePlatform.Application.Common.Interfaces.Services;

namespace AIFinancePlatform.Application.CQRS.Commands.Receipts.UploadReceipt;

public record UploadReceiptCommand(
    Guid UserId,
    Stream FileStream,
    string FileName
) : IRequest<UploadReceiptCommandResult>;

public class UploadReceiptCommandHandler : IRequestHandler<UploadReceiptCommand, UploadReceiptCommandResult>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IEventPublisher _eventPublisher;

    public UploadReceiptCommandHandler(
        IFileStorageService fileStorageService,
        IEventPublisher eventPublisher)
    {
        _fileStorageService = fileStorageService;
        _eventPublisher = eventPublisher;
    }

    public async Task<UploadReceiptCommandResult> Handle(UploadReceiptCommand request, CancellationToken cancellationToken)
    {
        // 1. Save file to disk via IFileStorageService
        var filePath = await _fileStorageService.SaveFileAsync(request.FileStream, request.FileName);

        // 2. Publish event to RabbitMQ via IEventPublisher
        var @event = new ReceiptUploadedEvent(
            request.UserId,
            filePath,
            request.FileName
        );

        await _eventPublisher.PublishAsync(@event, "receipt_queue_v2");

        return new UploadReceiptCommandResult(
            filePath,
            request.FileName,
            true,
            "Fiş başarıyla yüklendi ve kuyruğa alındı."
        );
    }
}
