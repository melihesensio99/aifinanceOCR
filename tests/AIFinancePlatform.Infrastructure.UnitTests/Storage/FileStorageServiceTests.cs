using System;
using System.IO;
using System.Threading.Tasks;
using AIFinancePlatform.Application.Common.Interfaces.Services;
using AIFinancePlatform.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AIFinancePlatform.Infrastructure.UnitTests.Storage;

public class FileStorageServiceTests
{
    private readonly LocalFileStorageService _service;

    public FileStorageServiceTests()
    {
        _service = new LocalFileStorageService();
    }

    [Fact]
    public async Task UploadAsync_ShouldGenerateValidLocalPath()
    {
        // Arrange
        var fileName = "test_image.jpg";
        var fileStream = new MemoryStream();

        // Act & Assert
        // The service should just throw because we are not using a valid physical file path in tests maybe,
        // or it will return a path.
        var path = await _service.UploadFileAsync(fileStream, fileName);
        path.Should().Contain(fileName);
    }
}
