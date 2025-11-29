using E_commerce.Application.Commands.Payments;
using FluentValidation;

namespace E_commerce.Application.Validators.Payments;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be uppercase letters only");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .Must(method => new[] { "CREDITCARD", "DEBITCARD", "PAYPAL", "STRIPE", "BANKTRANSFER", "CASHONDELIVERY" }
                .Contains(method.ToUpper()))
            .WithMessage("Invalid payment method");
    }
}
