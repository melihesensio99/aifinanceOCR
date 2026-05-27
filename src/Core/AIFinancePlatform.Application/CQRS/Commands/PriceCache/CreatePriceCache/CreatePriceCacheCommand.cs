using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AIFinancePlatform.Application.Common.Interfaces.Services;

namespace AIFinancePlatform.Application.CQRS.Commands.PriceCache.CreatePriceCache;

public record CreatePriceCacheCommand(string SearchTerm, string Price) : IRequest<Guid>;

public class CreatePriceCacheCommandHandler : IRequestHandler<CreatePriceCacheCommand, Guid>
{
    private readonly IRedisCacheService _redisCacheService;

    public CreatePriceCacheCommandHandler(IRedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    public async Task<Guid> Handle(CreatePriceCacheCommand request, CancellationToken cancellationToken)
    {
        var key = $"pricecache:{request.SearchTerm.ToLowerInvariant().Replace(" ", "_")}";
        
        // Cache süresi 7 gün
        await _redisCacheService.SetCacheValueAsync(key, request.Price, TimeSpan.FromDays(7));

        return Guid.NewGuid(); // Redis key-value kullandığı için id'ye çok gerek yok ama arayüz uyumu için sahte guid dönüyoruz.
    }
}
