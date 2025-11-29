using E_commerce.Domain.ValueObjects;

namespace E_commerce.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; private set; } // Customer's first name
    public string LastName { get; private set; } // Customer's last name
    public Email Email { get; private set; } // Customer's email address (Value Object - validated)
    public string? PhoneNumber { get; private set; } // Customer's phone number (optional)
    public Address? DefaultShippingAddress { get; private set; } // Default address for order delivery (optional)
    public Address? DefaultBillingAddress { get; private set; } // Default address for billing (optional)
    public bool IsActive { get; private set; } // Whether customer account is active (false = deactivated)
    public DateTime RegistrationDate { get; private set; } // When customer registered/signed up (UTC)

    private Customer() { } // EF Core

    public Customer(string firstName, string lastName, Email email, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PhoneNumber = phoneNumber;
        IsActive = true;
        RegistrationDate = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdateTimestamp();
    }

    public void UpdateEmail(Email email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        UpdateTimestamp();
    }

    public void SetDefaultShippingAddress(Address address)
    {
        DefaultShippingAddress = address ?? throw new ArgumentNullException(nameof(address));
        UpdateTimestamp();
    }

    public void SetDefaultBillingAddress(Address address)
    {
        DefaultBillingAddress = address ?? throw new ArgumentNullException(nameof(address));
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }
}
