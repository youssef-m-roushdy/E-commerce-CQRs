using E_commerce.Application.Commands.Payments;

namespace E_commerce.Application.DTOs;

public class CreatePaymentIntentDto
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string PaymentMethod { get; set; } = "Card";
}

public class CreatePaymentIntentResponseDto
{
    public Guid PaymentId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class ConfirmPaymentDto
{
    public string PaymentIntentId { get; set; } = string.Empty;
}
