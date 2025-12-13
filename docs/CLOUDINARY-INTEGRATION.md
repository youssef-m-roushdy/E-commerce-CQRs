# Cloudinary Image Upload Integration

## Overview
The E-Commerce API integrates with Cloudinary for professional image hosting and management. Both product images and customer profile pictures are stored in Cloudinary with automatic optimization and transformation.

## Features
- ✅ **2MB file size limit** for both product and profile images
- ✅ **Automatic image optimization** (quality and format)
- ✅ **Separate folders** for organization (products and profiles)
- ✅ **Image transformations** applied automatically
- ✅ **Secure uploads** with authentication
- ✅ **Allowed formats**: JPG, JPEG, PNG, GIF, WEBP

## Configuration

### 1. Cloudinary Account Setup
1. Create a free account at [cloudinary.com](https://cloudinary.com/)
2. Get your credentials from the Dashboard:
   - Cloud Name
   - API Key
   - API Secret

### 2. Update appsettings.json
```json
{
  "CloudinarySettings": {
    "CloudName": "your-cloudinary-cloud-name",
    "ApiKey": "your-cloudinary-api-key",
    "ApiSecret": "your-cloudinary-api-secret"
  }
}
```

## API Endpoints

### Upload Product Image
**Endpoint:** `POST /api/products/{id}/image`

**Authorization:** Admin, Manager

**Request:**
- Content-Type: `multipart/form-data`
- Body: Form data with `image` file

**Restrictions:**
- Max file size: **2MB**
- Allowed formats: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`

**Example (cURL):**
```bash
curl -X POST "http://localhost:5272/api/products/{productId}/image" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "image=@/path/to/product-image.jpg"
```

**Example (JavaScript/Fetch):**
```javascript
const uploadProductImage = async (productId, imageFile, token) => {
  const formData = new FormData();
  formData.append('image', imageFile);

  const response = await fetch(`http://localhost:5272/api/products/${productId}/image`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  return await response.json();
};

// Usage
const fileInput = document.getElementById('product-image');
const file = fileInput.files[0];
await uploadProductImage('product-guid-here', file, 'your-jwt-token');
```

**Product Image Specifications:**
- **Transformation:** Limited to 800x800 pixels (maintains aspect ratio)
- **Folder:** `ecommerce/products`
- **Quality:** Automatic optimization
- **Format:** Auto-converted to best format (WebP when supported)
- **Public ID:** `product_{productId}_{guid}`

**Success Response:**
```json
{
  "message": "Product image uploaded successfully"
}
```

### Upload Customer Profile Picture
**Endpoint:** `POST /api/customers/{id}/profile-picture`

**Authorization:** Customer (own profile), Admin, Manager

**Request:**
- Content-Type: `multipart/form-data`
- Body: Form data with `image` file

**Restrictions:**
- Max file size: **2MB**
- Allowed formats: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`

**Example (cURL):**
```bash
curl -X POST "http://localhost:5272/api/customers/{customerId}/profile-picture" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "image=@/path/to/profile-picture.jpg"
```

**Example (JavaScript/Fetch):**
```javascript
const uploadProfilePicture = async (customerId, imageFile, token) => {
  const formData = new FormData();
  formData.append('image', imageFile);

  const response = await fetch(`http://localhost:5272/api/customers/${customerId}/profile-picture`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  return await response.json();
};
```

**Profile Picture Specifications:**
- **Transformation:** Cropped to 500x500 pixels with face detection
- **Folder:** `ecommerce/profiles`
- **Crop Mode:** Fill with face gravity (focuses on faces)
- **Quality:** Automatic optimization
- **Format:** Auto-converted to best format
- **Public ID:** `user_{userId}_{guid}`

**Success Response:**
```json
{
  "message": "Profile picture uploaded successfully"
}
```

## React Component Example

### Product Image Upload Component
```typescript
import React, { useState } from 'react';

const ProductImageUpload: React.FC<{ productId: string; token: string }> = ({ productId, token }) => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    
    if (file) {
      // Validate file size (2MB)
      if (file.size > 2 * 1024 * 1024) {
        setError('File size must be less than 2MB');
        return;
      }

      // Validate file type
      const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
      if (!validTypes.includes(file.type)) {
        setError('Only JPG, PNG, GIF, and WebP images are allowed');
        return;
      }

      setSelectedFile(file);
      setError(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    setUploading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append('image', selectedFile);

      const response = await fetch(`http://localhost:5272/api/products/${productId}/image`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      });

      if (!response.ok) {
        throw new Error('Upload failed');
      }

      const result = await response.json();
      alert(result.message);
      setSelectedFile(null);
    } catch (err) {
      setError('Failed to upload image');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div>
      <input 
        type="file" 
        accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
        onChange={handleFileChange}
        disabled={uploading}
      />
      {selectedFile && (
        <div>
          <p>Selected: {selectedFile.name} ({(selectedFile.size / 1024).toFixed(2)} KB)</p>
          <button onClick={handleUpload} disabled={uploading}>
            {uploading ? 'Uploading...' : 'Upload Image'}
          </button>
        </div>
      )}
      {error && <p style={{ color: 'red' }}>{error}</p>}
    </div>
  );
};

export default ProductImageUpload;
```

### Profile Picture Upload Component
```typescript
import React, { useState } from 'react';

const ProfilePictureUpload: React.FC<{ customerId: string; token: string }> = ({ customerId, token }) => {
  const [preview, setPreview] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    
    if (!file) return;

    // Show preview
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreview(reader.result as string);
    };
    reader.readAsDataURL(file);

    // Auto-upload
    setUploading(true);
    const formData = new FormData();
    formData.append('image', file);

    try {
      const response = await fetch(`http://localhost:5272/api/customers/${customerId}/profile-picture`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      });

      const result = await response.json();
      console.log(result.message);
    } catch (error) {
      console.error('Upload failed', error);
    } finally {
      setUploading(false);
    }
  };

  return (
    <div>
      <label htmlFor="profile-pic" style={{ cursor: 'pointer' }}>
        {preview ? (
          <img src={preview} alt="Preview" style={{ width: 150, height: 150, borderRadius: '50%' }} />
        ) : (
          <div style={{ width: 150, height: 150, border: '2px dashed #ccc', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            {uploading ? 'Uploading...' : 'Click to upload'}
          </div>
        )}
      </label>
      <input 
        id="profile-pic"
        type="file" 
        accept="image/*"
        onChange={handleFileChange}
        style={{ display: 'none' }}
        disabled={uploading}
      />
    </div>
  );
};

export default ProfilePictureUpload;
```

## Error Handling

### Common Errors

**1. File Size Exceeded**
```json
{
  "message": "File size exceeds the maximum limit of 2MB. Current size: 2.5MB"
}
```

**2. Invalid File Type**
```json
{
  "message": "File type '.pdf' is not allowed. Allowed types: .jpg, .jpeg, .png, .gif, .webp"
}
```

**3. Missing File**
```json
{
  "message": "Image file is required"
}
```

**4. Product/Customer Not Found**
```json
{
  "message": "Product with ID {id} not found"
}
```

**5. Unauthorized**
```json
{
  "message": "Unauthorized access"
}
```

## Image Transformations

### Product Images
- **Size:** Limited to 800x800 (maintains aspect ratio)
- **Crop:** `limit` - Never crops, just scales down
- **Quality:** `auto` - Cloudinary optimizes automatically
- **Format:** `auto` - Best format for each browser (WebP, AVIF, etc.)

### Profile Pictures
- **Size:** Cropped to exactly 500x500
- **Crop:** `fill` - Fills the entire space
- **Gravity:** `face` - Focuses on faces when cropping
- **Quality:** `auto`
- **Format:** `auto`

## Cloudinary Dashboard

After uploading, images appear in your Cloudinary dashboard:
- **URL:** https://cloudinary.com/console
- **Media Library:** View all uploaded images
- **Folders:** 
  - `ecommerce/products` - All product images
  - `ecommerce/profiles` - All profile pictures

## Testing

### Using Postman

1. **Create New Request**
   - Method: POST
   - URL: `http://localhost:5272/api/products/{productId}/image`

2. **Add Authorization**
   - Type: Bearer Token
   - Token: Your JWT token

3. **Body**
   - Type: form-data
   - Key: `image` (change type to File)
   - Value: Select your image file

4. **Send Request**

### Using Thunder Client (VS Code)

1. Create new request
2. Set method to POST
3. Set URL
4. Auth tab → Bearer token
5. Body tab → Form → Add file with key `image`
6. Send

## Database Schema Update

A new column was added to the `Customers` table:

```sql
ALTER TABLE Customers
ADD ProfilePictureUrl NVARCHAR(500) NULL;
```

You'll need to create and run a migration:

```bash
cd src/E-commerce.Infrastructure
dotnet ef migrations add AddProfilePictureToCustomer
dotnet ef database update
```

## Security Considerations

1. **File Size Limit:** Hard-coded 2MB limit prevents large uploads
2. **File Type Validation:** Only image formats allowed
3. **Authentication Required:** All uploads require valid JWT token
4. **Role-Based Access:** 
   - Product images: Admin, Manager only
   - Profile pictures: User can upload own, Admin/Manager can upload any
5. **Cloudinary Account:** Keep API credentials secure in appsettings.json

## Performance Tips

1. **Client-side validation:** Check file size/type before uploading
2. **Image compression:** Compress images client-side before upload
3. **Progress indicators:** Show upload progress to users
4. **Lazy loading:** Load product images on demand
5. **Responsive images:** Use Cloudinary's URL parameters for different sizes

## Cloudinary URL Parameters

You can modify image URLs for different sizes:

```
Original: https://res.cloudinary.com/cloud-name/image/upload/v1234567890/ecommerce/products/product_123.jpg

Thumbnail (200x200):
https://res.cloudinary.com/cloud-name/image/upload/w_200,h_200,c_fill/v1234567890/ecommerce/products/product_123.jpg

Low quality preview:
https://res.cloudinary.com/cloud-name/image/upload/q_30,f_auto/v1234567890/ecommerce/products/product_123.jpg
```

## Next Steps

1. Configure your Cloudinary account
2. Update appsettings.json with credentials
3. Run database migration for ProfilePictureUrl column
4. Test uploads with Postman
5. Implement frontend upload components

## Support

For Cloudinary-specific issues:
- Documentation: https://cloudinary.com/documentation
- Support: https://support.cloudinary.com/

For API issues:
- Check error responses
- Verify JWT token is valid
- Ensure file meets requirements (size, type)
- Check user roles and permissions
