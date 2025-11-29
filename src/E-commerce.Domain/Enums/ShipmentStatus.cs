namespace E_commerce.Domain.Enums;

/// <summary>
/// Represents the status of a shipment/delivery
/// </summary>
public enum ShipmentStatus
{
    Pending = 0,
    PickedUp = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Failed = 5,
    Returned = 6
}
