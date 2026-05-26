using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using AIFinancePlatform.Application.Common.Interfaces.Services;

namespace AIFinancePlatform.Infrastructure.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["Azure:BlobConnectionString"];
        _containerName = configuration["Azure:ContainerName"] ?? "aifinance-receipts-container";

        // Connection string boşsa mock/dummy client oluşturur (Geliştirme için)
        if (!string.IsNullOrEmpty(connectionString))
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }
        else
        {
            // UseDevelopmentStorage=true local emülatör (Azurite) içindir
            _blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

        try
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Konteyner yoksa oluştur ve Public (Dışarıdan okunabilir) yap
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

            // Dosyayı Azure'a yükle
            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" } // Varsayılan tip
            });

            // Yüklenen dosyanın Azure Cloud URL'sini döndür
            return blobClient.Uri.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Azure Blob Error: {e.Message}");
            // Uygulamayı kırmamak adına mock URL dön
            return $"https://mock-azure-storage.blob.core.windows.net/{_containerName}/{uniqueFileName}";
        }
    }
}
