using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NashTech_TCG_API.Services.Interfaces
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        Task DeleteFileAsync(string fileUrl);
    }
}
