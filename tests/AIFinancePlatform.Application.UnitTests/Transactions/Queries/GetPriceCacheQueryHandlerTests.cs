using System.Threading;
using System.Threading.Tasks;
using AIFinancePlatform.Application.Common.Interfaces.Services;
using AIFinancePlatform.Application.CQRS.Queries.PriceCache.GetPriceCache;
using FluentAssertions;
using Moq;
using Xunit;

namespace AIFinancePlatform.Application.UnitTests.Transactions.Queries;

public class GetPriceCacheQueryHandlerTests
{
    private readonly Mock<IRedisCacheService> _mockRedisCache;
    private readonly GetPriceCacheQueryHandler _handler;

    public GetPriceCacheQueryHandlerTests()
    {
        _mockRedisCache = new Mock<IRedisCacheService>();
        _handler = new GetPriceCacheQueryHandler(_mockRedisCache.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPrice_WhenItemExistsInRedis()
    {
        // Arrange
        var searchTerm = "sutas_yogurt";
        var expectedPrice = "45.50";
        var expectedKey = $"pricecache:{searchTerm}";

        _mockRedisCache.Setup(r => r.GetCacheValueAsync(expectedKey))
                       .ReturnsAsync(expectedPrice);

        var query = new GetPriceCacheQuery(searchTerm);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedPrice);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        var searchTerm = "olmayan_urun";
        var expectedKey = $"pricecache:{searchTerm}";

        _mockRedisCache.Setup(r => r.GetCacheValueAsync(expectedKey))
                       .ReturnsAsync((string?)null);

        var query = new GetPriceCacheQuery(searchTerm);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeNull();
    }
}
