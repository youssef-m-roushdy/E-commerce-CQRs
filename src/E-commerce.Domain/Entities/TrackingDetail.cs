using E_commerce.Domain.Enums;

namespace E_commerce.Domain.Entities;

public class TrackingDetail : BaseEntity
{
    public Guid OrderId { get; private set; } // Foreign key to Order being tracked
    public DateTime StatusDate { get; private set; } // When this tracking status was recorded (UTC)
    public string Location { get; private set; } // Current location of shipment (e.g., "Memphis, TN")
    public string StatusDescription { get; private set; } // Human-readable status description
    public ShipmentStatus Status { get; private set; } // Shipment status (Pending, InTransit, Delivered, etc.)
    public string? Carrier { get; private set; } // Shipping carrier name (e.g., "FedEx", "UPS") - optional
    public string? TrackingNumber { get; private set; } // Carrier's tracking number - optional

    private TrackingDetail() { } // EF Core

    public TrackingDetail(Guid orderId, string location, string statusDescription, ShipmentStatus status, string? carrier = null, string? trackingNumber = null)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location cannot be empty", nameof(location));
        if (string.IsNullOrWhiteSpace(statusDescription))
            throw new ArgumentException("Status description cannot be empty", nameof(statusDescription));

        OrderId = orderId;
        StatusDate = DateTime.UtcNow;
        Location = location;
        StatusDescription = statusDescription;
        Status = status;
        Carrier = carrier;
        TrackingNumber = trackingNumber;
    }

    public void UpdateStatus(string location, string statusDescription, ShipmentStatus status)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        StatusDescription = statusDescription ?? throw new ArgumentNullException(nameof(statusDescription));
        Status = status;
        StatusDate = DateTime.UtcNow;
        UpdateTimestamp();
    }
}