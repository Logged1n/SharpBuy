using SharedKernel;
using SharedKernel.Dtos;

namespace Domain.Addresses;

public sealed class Address : Entity
{
    public Guid Id { get; set; }
    public string Line1 { get; set; }
    public string? Line2 { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }

    public Address() { }
    public Address(AddressDto dto)
    {
        Line1 = dto.Line1;
        Line2 = dto.Line2;
        City = dto.City;
        PostalCode = dto.PostalCode;
        Country = dto.Country;
    }
}
