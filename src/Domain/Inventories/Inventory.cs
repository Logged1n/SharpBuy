using Domain.Products;
using SharedKernel;

namespace Domain.Inventories;

public sealed class Inventory : Entity
{
    public Guid Id { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public DateTime LastUpdated { get; set; }
}
