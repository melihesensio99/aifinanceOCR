using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Application.Common.Models;
using AIFinancePlatform.Application.CQRS.Queries.Transactions.GetTransactions;
using AIFinancePlatform.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace AIFinancePlatform.Application.UnitTests.Transactions.Queries;

public class GetTransactionsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly GetTransactionsQueryHandler _handler;

    public GetTransactionsQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new GetTransactionsQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Veritabanında sanki 10 tane Transaction varmış gibi sahte bir liste hazırlıyoruz
        var fakeTransactions = new List<Transaction>();
        for (int i = 1; i <= 10; i++)
        {
            fakeTransactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = $"Test Fişi {i}",
                Amount = 100 * i,
                Date = DateTime.UtcNow.AddDays(-i)
            });
        }

        // EF Core'un LINQ sorgularını taklit edebilmesi için MockQueryable kullanıyoruz
        var mockDbSet = fakeTransactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        // Act: Sayfa 2 ve Sayfa Boyutu 3 isteyelim (1,2,3 atlanacak -> 4,5,6 gelmeli)
        var query = new GetTransactionsQuery(userId, PageNumber: 2, PageSize: 3);
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(10);
        result.TotalPages.Should().Be(4); // 10/3 = 3.33 -> 4 sayfa
        result.HasPreviousPage.Should().BeTrue(); // 2. sayfadayız, öncesi var
        result.HasNextPage.Should().BeTrue();     // 2. sayfadayız, sonrası var
        
        result.Items.Should().HaveCount(3);
        result.Items[0].Title.Should().Be("Test Fişi 4");
        result.Items[1].Title.Should().Be("Test Fişi 5");
        result.Items[2].Title.Should().Be("Test Fişi 6");
    }
}
