using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Inmobiliaria.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string?> SubirImagenAsync(IFormFile archivo, string carpeta = "inmobiliaria")
        {
            if (archivo == null || archivo.Length == 0) return null;

            using var stream = archivo.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(archivo.FileName, stream),
                Folder = carpeta,
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl?.ToString();
        }

        public async Task<bool> EliminarImagenAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return false;
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
    }
}