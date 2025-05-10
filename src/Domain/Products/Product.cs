using Domain.Categories;
using Domain.Reviews;
using SharedKernel;
using SharedKernel.ValueObjects;

namespace Domain.Products;

public sealed class Product : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Money Price { get; set; }
    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public string MainPhotoPath { get; set; } = string.Empty;
    public ICollection<string> PhotoPaths { get; set; } = [];
}
