using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Customers;

public class UpdateCustomerProfilePictureCommandHandler : ICommandHandler<UpdateCustomerProfilePictureCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;

    public UpdateCustomerProfilePictureCommandHandler(IApplicationDbContext context, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<bool> Handle(UpdateCustomerProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && !c.IsDeleted, cancellationToken);

        if (customer == null)
            return false;

        // Upload image to Cloudinary
        var imageUrl = await _cloudinaryService.UploadUserProfilePictureAsync(
            request.ProfilePicture,
            customer.Id.ToString(),
            cancellationToken);

        // Update customer profile picture (you may need to add this property to Customer entity)
        customer.UpdateProfilePicture(imageUrl);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
