using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Domain.Entities;

namespace AIFinancePlatform.Application.CQRS.Commands.PriceCache.CreatePriceCache;

public record CreatePriceCacheCommand(string SearchTerm, string Price) : IRequest<Guid>;

public class CreatePriceCacheCommandHandler : IRequestHandler<CreatePriceCacheCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePriceCacheCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePriceCacheCommand request, CancellationToken cancellationToken)
    {
        var cache = new ProductPriceCache
        {
            Id = Guid.NewGuid(),
            SearchTerm = request.SearchTerm,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductPriceCaches.Add(cache);
        await _context.SaveChangesAsync(cancellationToken);

        return cache.Id;
    }
}
