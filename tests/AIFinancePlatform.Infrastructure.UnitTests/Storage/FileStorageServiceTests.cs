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
    private readonly Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment> _mockEnv;

    public FileStorageServiceTests()
    {
        _mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        _mockEnv.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
        _service = new LocalFileStorageService(_mockEnv.Object);
    }

    [Fact]
    public async Task SaveFileAsync_ShouldGenerateValidLocalPath()
    {
        // Arrange
        var fileName = "test_image.jpg";
        var fileStream = new MemoryStream();

        // Act
        var path = await _service.SaveFileAsync(fileStream, fileName);
        
        // Assert
        path.Should().Contain(fileName);
    }
}
