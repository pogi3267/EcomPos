using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace ApplicationWeb.HelperAndConstant
{
    public class ImageProcessing : IImageProcessing
    {
        private readonly IWebHostEnvironment _environment;

        public ImageProcessing(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string GetImagePath(IFormFile photo, string folderName)
        {
            string path = Path.Combine(_environment.WebRootPath, folderName);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Path.Combine(path, fileName);
        }

        public string GetImagePath(string file, string folderName)
        {
            string path = Path.Combine(_environment.WebRootPath, folderName);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Path.Combine(path, fileName);
        }

        public string GetImagePathForDb(string imagePath)
        {
            string webRootFolder = _environment.WebRootPath;
            imagePath = imagePath.Replace(webRootFolder, "");
            return imagePath.Replace("\\", "/");
        }
    }
}
