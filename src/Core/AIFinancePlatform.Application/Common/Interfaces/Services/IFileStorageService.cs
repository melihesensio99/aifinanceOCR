using System.IO;
using System.Threading.Tasks;

namespace AIFinancePlatform.Application.Common.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName);
}
