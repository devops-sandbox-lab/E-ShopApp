using Microsoft.AspNetCore.Http;

namespace Application.Helpers
{
    public static class ImageSavingHelper
    {
        private static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        private static long maxAllowedImageSize =4* 1_048_576;

        public static async Task<List<string>> SaveImagesAsync(List<IFormFile> files, string folderName)
        {
            List<string> imageNames = new List<string>();

            if (files == null || files.Count == 0)
            {
                return imageNames;
            }

            var uploads = Path.Combine(BasePath, "Images", folderName);

            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            foreach (var file in files)
            {
                if (file.Length > 0 && file.Length <= maxAllowedImageSize)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploads, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    imageNames.Add($"/Images/{folderName}/{uniqueFileName}");
                }
                else
                {
                    throw new ApplicationException($"Image size exceeds the maximum allowed size (4 MB) !");
                }
            }
             
            return imageNames;
        }

        public static async Task<string> SaveOneImageAsync(IFormFile file, string folderName)
        {
            var imageNames = await SaveImagesAsync(new List<IFormFile> { file }, folderName);
            return $"{imageNames.FirstOrDefault()}";
        }
    }
}
