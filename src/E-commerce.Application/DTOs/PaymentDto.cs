namespace E_commerce.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
}
