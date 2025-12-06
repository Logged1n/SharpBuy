using Domain.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Products;

public sealed class UploadImage : IEndpoint
{
    private static readonly string[] AllowedExtensions = [".JPG", ".JPEG", ".PNG", ".GIF", ".WEBP"];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/upload-image", async (
            IFormFile file,
            IWebHostEnvironment environment) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("No file uploaded");

            // Validate file type
            string extension = Path.GetExtension(file.FileName).ToUpperInvariant();

            if (!AllowedExtensions.Contains(extension))
                return Results.BadRequest("Invalid file type. Only images are allowed.");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return Results.BadRequest("File size exceeds 5MB limit");

            // Create uploads directory if it doesn't exist
            string uploadsPath = Path.Combine(environment.ContentRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            string fileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path
            string relativePath = $"/uploads/products/{fileName}";
            return Results.Ok(new { path = relativePath });
        })
        .WithTags(Tags.Products)
        .RequireRoles(Roles.Admin, Roles.Salesman)
        .DisableAntiforgery();
    }
}
