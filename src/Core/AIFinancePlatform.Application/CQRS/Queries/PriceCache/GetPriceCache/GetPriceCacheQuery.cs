using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AIFinancePlatform.Application.Common.Interfaces.Services;

using AIFinancePlatform.Application.Common.Models;

namespace AIFinancePlatform.Application.CQRS.Queries.PriceCache.GetPriceCache;

public record GetPriceCacheQuery(string SearchTerm) : IRequest<Result<string?>>;

public class GetPriceCacheQueryHandler : IRequestHandler<GetPriceCacheQuery, Result<string?>>
{
    private readonly IRedisCacheService _redisCacheService;

    public GetPriceCacheQueryHandler(IRedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    public async Task<Result<string?>> Handle(GetPriceCacheQuery request, CancellationToken cancellationToken)
    {
        var key = $"pricecache:{request.SearchTerm.ToLowerInvariant().Replace(" ", "_")}";
        var price = await _redisCacheService.GetCacheValueAsync(key);
        return Result<string?>.Success(price);
    }
}
