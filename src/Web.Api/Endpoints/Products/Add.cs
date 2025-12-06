using System.Globalization;
using Application.Abstractions.Messaging;
using Application.Abstractions.Storage;
using Application.Products.Add;
using Domain.Users;
using SharedKernel;
using SharedKernel.ValueObjects;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

public sealed class Add : IEndpoint
{
    private static readonly string[] AllowedExtensions = [".JPG", ".JPEG", ".PNG", ".GIF", ".WEBP"];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products", async (
            HttpRequest httpRequest,
            ICommandHandler<AddProductCommand, Guid> handler,
            IFileStorageService fileStorageService,
            CancellationToken cancellationToken) =>
        {
            if (!httpRequest.HasFormContentType)
                return Results.BadRequest("Request must be multipart/form-data");

            IFormCollection form = await httpRequest.ReadFormAsync(cancellationToken);

            // Read form fields
            string name = form["name"].ToString();
            string description = form["description"].ToString();
            int quantity = int.Parse(form["quantity"].ToString(), CultureInfo.InvariantCulture);
            decimal priceAmount = decimal.Parse(form["priceAmount"].ToString(), CultureInfo.InvariantCulture);
            string priceCurrency = form["priceCurrency"].ToString();
            string categoryIdsStr = form["categoryIds"].ToString();

            List<Guid> categoryIds = string.IsNullOrEmpty(categoryIdsStr)
                ? []
                : categoryIdsStr.Split(',').Select(Guid.Parse).ToList();

            // Handle image upload
            string mainPhotoPath = "/placeholder.jpg";
            IFormFile? file = form.Files.GetFile("image");
            if (file != null && file.Length > 0)
            {
                // Validate file
                string extension = Path.GetExtension(file.FileName).ToUpperInvariant();

                if (!AllowedExtensions.Contains(extension))
                    return Results.BadRequest("Invalid file type. Only images are allowed.");

                if (file.Length > 5 * 1024 * 1024)
                    return Results.BadRequest("File size exceeds 5MB limit");

                using Stream stream = file.OpenReadStream();
                mainPhotoPath = await fileStorageService.SaveProductImageAsync(stream, file.FileName, cancellationToken);
            }

            AddProductCommand command = new(
                name,
                description,
                quantity,
                new Money(priceAmount, priceCurrency),
                categoryIds,
                mainPhotoPath);

            Result<Guid> result = await handler.Handle(command, cancellationToken);
            return result.Match(
                id => Results.Created($"/products/{id}", id),
                CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .RequireRoles(Roles.Admin, Roles.Salesman);
    }
}
