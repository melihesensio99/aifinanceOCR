using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

using AIFinancePlatform.Application.Common.Models;

namespace AIFinancePlatform.Application.CQRS.Queries.Transactions.GetTransactionsPdf;

// 1. Dışarıdan gelecek İSTEK (Query)
// Geriye byte[] (PDF dosyasının bayt hali) döndürecek.
public record GetTransactionsPdfQuery(Guid UserId) : IRequest<Result<byte[]>>;

// 2. İsteği yakalayacak İŞLEYİCİ (Handler)
public class GetTransactionsPdfQueryHandler : IRequestHandler<GetTransactionsPdfQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsPdfQueryHandler(IApplicationDbContext context)
    {
        _context = context;
        // QuestPDF ücretsiz kullanım lisans onayı
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<Result<byte[]>> Handle(GetTransactionsPdfQuery request, CancellationToken cancellationToken)
    {
        // 1. Veritabanından kullanıcının işlemlerini çekiyoruz
        var transactionList = await _context.Transactions
            .Where(t => t.UserId == request.UserId)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);

        // 2. QuestPDF ile harika bir PDF çiziyoruz
        var pdfDocument = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                // BAŞLIK KISMI
                page.Header().Text("AI Finance Platform - Harcama Raporu")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                // İÇERİK KISMI (Tablo)
                page.Content().PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(); // Tarih
                        columns.RelativeColumn(); // Başlık
                        columns.RelativeColumn(); // Tip
                        columns.RelativeColumn(); // Tutar
                    });

                    // Tablo Başlıkları
                    table.Header(header =>
                    {
                        header.Cell().Text("Tarih").SemiBold();
                        header.Cell().Text("Aciklama").SemiBold();
                        header.Cell().Text("Tip").SemiBold();
                        header.Cell().Text("Tutar (TL)").SemiBold().AlignRight();
                    });

                    // Tablo Verileri (Döngü ile)
                    foreach (var item in transactionList)
                    {
                        table.Cell().Text(item.Date.ToShortDateString());
                        table.Cell().Text(item.Title);
                        table.Cell().Text(item.Type.ToString());
                        table.Cell().Text($"{item.Amount:N2}").AlignRight();
                    }
                });

                // ALT BİLGİ KISMI
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Sayfa ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return Result<byte[]>.Success(pdfDocument.GeneratePdf());
    }
}
