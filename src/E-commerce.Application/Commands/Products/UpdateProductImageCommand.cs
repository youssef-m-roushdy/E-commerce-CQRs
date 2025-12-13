using E_commerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace E_commerce.Application.Commands.Products;

public record UpdateProductImageCommand(
    Guid Id,
    IFormFile ImageFile
) : ICommand<bool>;
