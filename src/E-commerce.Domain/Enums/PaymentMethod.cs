namespace E_commerce.Domain.Enums;

/// <summary>
/// Represents payment methods available in the system
/// </summary>
public enum PaymentMethod
{
    CreditCard = 0,
    DebitCard = 1,
    PayPal = 2,
    BankTransfer = 3,
    CashOnDelivery = 4,
    Stripe = 5,
    ApplePay = 6,
    GooglePay = 7
}
