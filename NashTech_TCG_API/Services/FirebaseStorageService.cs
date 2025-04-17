using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NashTech_TCG_API.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NashTech_TCG_API.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly string _storageBucket;
        private readonly string _apiKey;
        private readonly ILogger<FirebaseStorageService> _logger;

        public FirebaseStorageService(IConfiguration configuration, ILogger<FirebaseStorageService> logger)
        {
            _storageBucket = configuration["Firebase:StorageBucket"];
            _apiKey = configuration["Firebase:ApiKey"];
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                // Generate a unique filename to avoid conflicts
                string fileExtension = Path.GetExtension(file.FileName);
                string fileName = $"{Guid.NewGuid()}{fileExtension}";

                // Define storage path
                string storagePath = $"{folderName}/{fileName}";

                // Create a Firebase Storage reference
                var storage = new FirebaseStorage(_storageBucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(_apiKey),
                        ThrowOnCancel = true
                    });

                // Upload the file to Firebase Storage
                using (var stream = file.OpenReadStream())
                {
                    var downloadUrl = await storage
                        .Child(storagePath)
                        .PutAsync(stream);

                    _logger.LogInformation($"File uploaded successfully. URL: {downloadUrl}");
                    return downloadUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Firebase Storage");
                return null;
            }
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return;

            try
            {
                // Extract the path from the URL
                Uri uri = new Uri(fileUrl);

                // Parse the path from Firebase URL
                string path = uri.AbsolutePath;

                // Firebase URLs look like: https://firebasestorage.googleapis.com/v0/b/[bucket]/o/[encoded-filepath]?[token]
                // We need to extract the part after "/o/" and decode it
                int startIndex = path.IndexOf("/o/") + 3;
                if (startIndex > 3)
                {
                    path = path.Substring(startIndex);
                    path = Uri.UnescapeDataString(path);

                    _logger.LogInformation($"Attempting to delete file at path: {path}");

                    var storage = new FirebaseStorage(_storageBucket,
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(_apiKey),
                            ThrowOnCancel = true
                        });

                    // Delete the file
                    await storage.Child(path).DeleteAsync();
                    _logger.LogInformation($"File deleted successfully from path: {path}");
                }
                else
                {
                    _logger.LogWarning($"Could not parse file path from URL: {fileUrl}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file from Firebase Storage: {fileUrl}");
            }
        }
    }
}
