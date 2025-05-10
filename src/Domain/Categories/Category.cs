using Domain.Products;
using SharedKernel;

namespace Domain.Categories;

public sealed class Category : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<Product> Products { get; set; } = [];
}
