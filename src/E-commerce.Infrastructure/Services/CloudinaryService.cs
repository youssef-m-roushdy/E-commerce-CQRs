using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace E_commerce.Infrastructure.Services;

/// <summary>
/// Cloudinary image upload service
/// </summary>
public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public CloudinaryService(IOptions<CloudinarySettings> settings)
    {
        var account = new Account(
            settings.Value.CloudName,
            settings.Value.ApiKey,
            settings.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadUserProfilePictureAsync(IFormFile file, string userId, CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            Folder = "ecommerce/profiles",
            PublicId = $"user_{userId}_{Guid.NewGuid()}",
            Transformation = new Transformation()
                .Width(500)
                .Height(500)
                .Crop("fill")
                .Gravity("face")
                .Quality("auto")
                .FetchFormat("auto"),
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error != null)
        {
            throw new InvalidOperationException($"Image upload failed: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }

    public async Task<string> UploadProductImageAsync(IFormFile file, string productId, CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            Folder = "ecommerce/products",
            PublicId = $"product_{productId}_{Guid.NewGuid()}",
            Transformation = new Transformation()
                .Width(800)
                .Height(800)
                .Crop("limit")
                .Quality("auto")
                .FetchFormat("auto"),
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error != null)
        {
            throw new InvalidOperationException($"Image upload failed: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }

    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return false;

        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        return result.Result == "ok";
    }

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException($"File size exceeds the maximum limit of 2MB. Current size: {file.Length / 1024.0 / 1024.0:F2}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
        }
    }
}
