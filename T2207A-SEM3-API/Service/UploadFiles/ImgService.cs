using Azure.Core;
using T2207A_SEM3_API.Entities;

namespace T2207A_SEM3_API.Service.UploadFiles
{
    public class ImgService : IImgService
    {
        public async Task<string> UploadImageAsync(IFormFile avatar)
        {
            if (avatar != null && avatar.Length > 0)
            {
                string fileName = GenerateUniqueFileName(avatar);

                string uploadDirectory = GetUploadDirectory();
                string filePath = Path.Combine(uploadDirectory, fileName);

                Directory.CreateDirectory(uploadDirectory);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                return GenerateFileUrl(fileName);
            }

            return "https://localhost:7218/uploads/5d15b8f9-f4ba-4e05-aa7a-48d4ac5c91e3.png"; // Return null if no image is provided.
        }

        private string GenerateUniqueFileName(IFormFile avatar)
        {
            return $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";
        }

        private string GetUploadDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        }

        private string GenerateFileUrl(string fileName)
        {
            // You will need to provide the base URL here or retrieve it from your configuration.
            string baseUrl = "https://localhost:7218"; // Replace with your actual base URL.
            return $"{baseUrl}/uploads/{fileName}";
        }
    }
}
