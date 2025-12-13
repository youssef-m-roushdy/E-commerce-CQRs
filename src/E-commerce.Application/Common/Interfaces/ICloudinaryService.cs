using Microsoft.AspNetCore.Http;

namespace E_commerce.Application.Common.Interfaces;

/// <summary>
/// Service for uploading images to Cloudinary
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    /// Upload user profile picture (max 2MB)
    /// </summary>
    Task<string> UploadUserProfilePictureAsync(IFormFile file, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload product image (max 2MB)
    /// </summary>
    Task<string> UploadProductImageAsync(IFormFile file, string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete image from Cloudinary
    /// </summary>
    Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}
