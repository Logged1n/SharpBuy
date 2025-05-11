namespace SharedKernel.Dtos;

public sealed record AddressDto(
    string Line1,
    string? Line2,
    string City,
    string PostalCode,
    string Country);
