using Microsoft.AspNetCore.Http;

namespace Eshop.Application.Helpers
{
    public static class DocumentSavingHelper
    {
        private static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "docs");
        private static readonly HashSet<string> AllowedExtensions = new HashSet<string> { ".pdf", ".docx", ".csv" };

        static DocumentSavingHelper()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
        }

        public static async Task<string> SaveOneDocumentAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            var fileExtension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(fileExtension.ToLower()))
            {
                throw new ArgumentException("Unsupported file format.");
            }

            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            var uploads = Path.Combine(BasePath, folderName);

            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var filePath = Path.Combine(uploads, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/docs/{folderName}/{uniqueFileName}";
        }

        public static async Task<List<string>> SaveDocumentsAsync(List<IFormFile> files, string folderName)
        {
            List<string> fileNames = new List<string>();

            foreach (var file in files)
            {
                var fileName = await SaveOneDocumentAsync(file, folderName);
                fileNames.Add(fileName);
            }

            return fileNames;
        }
    }
}
