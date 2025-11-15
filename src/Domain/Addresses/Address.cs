using SharedKernel;
using SharedKernel.Dtos;

namespace Domain.Addresses;

public sealed class Address : Entity
{
    private Address() { }

    public Guid Id { get; private set; }
    public string Line1 { get; private set; }
    public string? Line2 { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }
    public Guid UserId { get; set; }

    public static Address Create(AddressDto dto)
    {
        return new Address()
        {
            Id = Guid.NewGuid(),
            Line1 = dto.Line1,
            Line2 = dto.Line2,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
        };
    }

    public static Address Create(string line1, string? line2, string city, string postalCode, string country)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(line1);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(city);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(postalCode);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(country);

        return new Address()
        {
            Id = Guid.NewGuid(),
            Line1 = line1,
            Line2 = line2,
            City = city,
            PostalCode = postalCode,
            Country = country,
        };
    }

    public Result Update(
        string? line1,
        string? line2,
        string? city,
        string? postalCode,
        string? country)
    {
        if (!string.IsNullOrWhiteSpace(line1))
            Line1 = line1;

        if (!string.IsNullOrWhiteSpace(line2))
            Line2 = line2;

        if (!string.IsNullOrWhiteSpace(city))
            City = city;

        if (!string.IsNullOrWhiteSpace(postalCode))
            PostalCode = postalCode;

        if (!string.IsNullOrWhiteSpace(country))
            Country = country;

        return Result.Success();
    }
}
