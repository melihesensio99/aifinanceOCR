using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AIFinancePlatform.IntegrationTests.Controllers;

public class TransactionControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TransactionControllerTests(WebApplicationFactory<Program> factory)
    {
        // Sanal bir HTTP Client (Postman gibi) oluşturuyoruz.
        // Bu client, In-Memory ayağa kalkan API'mize istek atacak.
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTransactions_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        // Arrange
        var requestUri = "/api/transaction?pageNumber=1&pageSize=10";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        // API'mizde [Authorize] etiketi olduğu için, token göndermeden istek atarsak 401 Unauthorized dönmeli!
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
