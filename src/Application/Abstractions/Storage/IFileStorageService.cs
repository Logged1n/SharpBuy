namespace Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> SaveProductImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteProductImageAsync(string filePath, CancellationToken cancellationToken = default);
}
