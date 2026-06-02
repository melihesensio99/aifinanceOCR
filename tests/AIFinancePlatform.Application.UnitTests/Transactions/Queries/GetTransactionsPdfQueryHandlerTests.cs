using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Application.CQRS.Queries.Transactions.GetTransactionsPdf;
using AIFinancePlatform.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace AIFinancePlatform.Application.UnitTests.Transactions.Queries;

public class GetTransactionsPdfQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly GetTransactionsPdfQueryHandler _handler;

    public GetTransactionsPdfQueryHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _handler = new GetTransactionsPdfQueryHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPdfByteArray_WhenTransactionsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var fakeTransactions = new List<Transaction>
        {
            new Transaction { Id = Guid.NewGuid(), UserId = userId, Title = "Market", Amount = 150, Date = DateTime.UtcNow },
            new Transaction { Id = Guid.NewGuid(), UserId = userId, Title = "Fatura", Amount = 200, Date = DateTime.UtcNow.AddDays(-1) }
        };

        var mockDbSet = fakeTransactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetTransactionsPdfQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<byte[]>();
        result.Length.Should().BeGreaterThan(0);
        
        // PDF dosyalarının sihirli baytları (Magic Bytes) '%PDF' ile başlar.
        // Bu byte dizisinin 37 80 68 70 olduğuna emin olabiliriz.
        result[0].Should().Be(0x25); // %
        result[1].Should().Be(0x50); // P
        result[2].Should().Be(0x44); // D
        result[3].Should().Be(0x46); // F
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPdf_WhenUserHasNoTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeTransactions = new List<Transaction>(); // Boş liste

        var mockDbSet = fakeTransactions.BuildMockDbSet();
        _mockContext.Setup(c => c.Transactions).Returns(mockDbSet.Object);

        var query = new GetTransactionsPdfQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0); // İçinde kayıt olmasa da boş PDF tablosu çizilir
    }
}
