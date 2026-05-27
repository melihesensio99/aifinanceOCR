using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using AIFinancePlatform.Application.Common.Interfaces.Persistence;
using AIFinancePlatform.Application.Common.Interfaces.Events;
using AIFinancePlatform.Application.CQRS.Commands.Transactions.CreateTransaction;

namespace AIFinancePlatform.Application.UnitTests.Transactions.Commands;

public class CreateTransactionCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();

        _handler = new CreateTransactionCommandHandler(
            _mockContext.Object
        );
    }

    [Fact]
    public void Validator_ShouldFail_WhenAmountIsLessThanZero()
    {
        // Arrange
        var command = new CreateTransactionCommand(
            UserId: Guid.NewGuid(),
            Title: "Migros Alışveriş",
            Amount: -50, // Hata fırlatmasını beklediğimiz eksi değer!
            Type: "Expense",
            Date: DateTime.UtcNow,
            Description: "Hatalı işlem",
            CategoryId: Guid.NewGuid(),
            IsAutomatic: false,
            Source: "Manual",
            ReceiptImageUrl: null
        );

        var validator = new CreateTransactionCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Tutar 0'dan büyük olmalıdır.");
    }
}
