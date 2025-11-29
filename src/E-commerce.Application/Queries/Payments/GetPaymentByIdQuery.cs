using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;

namespace E_commerce.Application.Queries.Payments;

public record GetPaymentByIdQuery(Guid Id) : IQuery<PaymentDto?>;
