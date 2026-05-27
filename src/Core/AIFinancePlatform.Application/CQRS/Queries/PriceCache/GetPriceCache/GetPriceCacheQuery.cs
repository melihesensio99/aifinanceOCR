using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AIFinancePlatform.Application.Common.Interfaces.Services;

namespace AIFinancePlatform.Application.CQRS.Queries.PriceCache.GetPriceCache;

public record GetPriceCacheQuery(string SearchTerm) : IRequest<string?>;

public class GetPriceCacheQueryHandler : IRequestHandler<GetPriceCacheQuery, string?>
{
    private readonly IRedisCacheService _redisCacheService;

    public GetPriceCacheQueryHandler(IRedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    public async Task<string?> Handle(GetPriceCacheQuery request, CancellationToken cancellationToken)
    {
        var key = $"pricecache:{request.SearchTerm.ToLowerInvariant().Replace(" ", "_")}";
        var price = await _redisCacheService.GetCacheValueAsync(key);
        return price;
    }
}
