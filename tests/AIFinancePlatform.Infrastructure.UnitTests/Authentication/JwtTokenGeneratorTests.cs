using System;
using AIFinancePlatform.Application.Common.Interfaces.Authentication;
using AIFinancePlatform.Domain.Entities;
using AIFinancePlatform.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AIFinancePlatform.Infrastructure.UnitTests.Authentication;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        var jwtSettings = new JwtSettings
        {
            Secret = "SuperSecretKeyThatIsAtLeast32BytesLongForHS256!!!",
            Issuer = "AIFinancePlatform",
            Audience = "AIFinancePlatformFrontEnd",
            ExpiryMinutes = 60
        };

        var mockOptions = Options.Create(jwtSettings);
        _generator = new JwtTokenGenerator(mockOptions);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtFormat()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        // Act
        var token = _generator.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Length.Should().Be(3); // JWT her zaman 3 parçadan oluşur (Header.Payload.Signature)
    }
}
