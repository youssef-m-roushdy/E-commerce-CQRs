using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Products;

public class UpdateProductImageCommandHandler : ICommandHandler<UpdateProductImageCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;

    public UpdateProductImageCommandHandler(IApplicationDbContext context, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<bool> Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
            return false;

        // Upload image to Cloudinary
        var imageUrl = await _cloudinaryService.UploadProductImageAsync(
            request.ImageFile,
            product.Id.ToString(),
            cancellationToken);

        product.UpdateImage(imageUrl);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
