using E_commerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace E_commerce.Application.Commands.Customers;

public record UpdateCustomerProfilePictureCommand(
    Guid CustomerId,
    IFormFile ProfilePicture
) : ICommand<bool>;
