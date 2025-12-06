using Application.Abstractions.Storage;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Storage;

internal sealed class FileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    public async Task<string> SaveProductImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        string uploadsPath = Path.Combine(environment.ContentRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadsPath);

        string extension = Path.GetExtension(fileName).ToUpperInvariant();
        string uniqueFileName = $"{Guid.NewGuid()}{extension}";
        string filePath = Path.Combine(uploadsPath, uniqueFileName);

        using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);
        }

        return $"/uploads/products/{uniqueFileName}";
    }

    public Task DeleteProductImageAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath) || filePath == "/placeholder.jpg")
            return Task.CompletedTask;

        string fullPath = Path.Combine(environment.ContentRootPath, filePath.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
