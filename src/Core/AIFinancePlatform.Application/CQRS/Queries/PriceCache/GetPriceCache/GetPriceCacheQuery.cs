using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Domain.Entities;

namespace AIFinancePlatform.Application.CQRS.Queries.PriceCache.GetPriceCache;

public record GetPriceCacheQuery(string SearchTerm) : IRequest<string>;

public class GetPriceCacheQueryHandler : IRequestHandler<GetPriceCacheQuery, string>
{
    private readonly IApplicationDbContext _context;

    public GetPriceCacheQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GetPriceCacheQuery request, CancellationToken cancellationToken)
    {
        // 24 saatten eski olmayan en güncel sonucu getir
        var cache = await _context.ProductPriceCaches
            .Where(c => c.SearchTerm == request.SearchTerm && c.CreatedAt >= DateTime.UtcNow.AddHours(-24))
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return cache?.Price;
    }
}
