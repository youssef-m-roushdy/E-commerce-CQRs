namespace E_commerce.Domain.ValueObjects;

/// <summary>
/// Address Value Object - Immutable and compared by value
/// Use this for customer addresses, shipping addresses, etc.
/// </summary>
public class Address : ValueObject
{
    public string Street { get; private init; }
    public string City { get; private init; }
    public string State { get; private init; }
    public string ZipCode { get; private init; }
    public string Country { get; private init; }

    private Address() { } // For EF Core

    public Address(string street, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode cannot be empty", nameof(zipCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        Street = street;
        City = city;
        State = state ?? string.Empty;
        ZipCode = zipCode;
        Country = country;
    }

    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }

    public override string ToString() => FullAddress;
}
