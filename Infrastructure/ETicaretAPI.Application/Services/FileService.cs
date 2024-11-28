using ETicaretAPI.Application.Services;
using ETicaretAPI.Infrastructure.Operations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETicaretAPI.Infrastructure.Services
{
    public class FileService : IFileService
    {
        readonly IWebHostEnvironment _webHostEnvironment;
        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<bool> CopyFileAsync(string path, IFormFile file)
        {
            try
            {
                // Dosyayı yazma için FileStream açıyoruz
                await using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: true);

                // IFormFile içeriğini FileStream'e kopyalıyoruz
                await file.CopyToAsync(fileStream);  // Dosyanın içeriğini doğrudan fileStream'e kopyala

                return true;
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama ve tekrar fırlatma işlemi yapılabilir
                throw new Exception("An error occurred while copying the file.", ex);
            }
        }



        public async Task<string> FileRenameAsync(string fileName, string directory)
        {
            // Dosya adını ve uzantısını ayırma
            string extension = Path.GetExtension(fileName); // Uzantıyı al
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName); // Adı ayır

            // Dosya yolunu oluşturma
            string fullPath = Path.Combine(directory, fileName);

            // Aynı isimde dosya varsa, -1, -2 şeklinde sayıyı ekleyin (ilk dosya için -1 değil, ilk sırada direkt dosya kaydedilecek)
            int count = 1; // Sayacın başlangıç değeri 1 olacak
            while (File.Exists(fullPath))
            {
                // Sayıyı ekleyerek yeni dosya ismini oluştur
                string newFileName = $"{fileNameWithoutExtension}-{count}{extension}";
                fullPath = Path.Combine(directory, newFileName);
                count++;
            }

            // Yeni dosya adını döndür
            return count == 1 ? fileName : $"{fileNameWithoutExtension}-{count - 1}{extension}";
        }




        public async Task<List<(string fileName, string path)>> UploadAsync(string path, IFormFileCollection files)
        {
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, path);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            List<(string fileName, string path)> datas = new();
            List<bool> results = new();

            foreach (IFormFile file in files)
            {
                // Dosya adını yeniden adlandırıyoruz
                string fileNewName = await FileRenameAsync(file.FileName, uploadPath);

                // Dosyayı kopyalıyoruz
                bool result = await CopyFileAsync($"{uploadPath}\\{fileNewName}", file);

                // Dosya bilgilerini kaydediyoruz
                datas.Add((fileNewName, $"{uploadPath}\\{fileNewName}"));
                results.Add(result);
            }

            // Eğer tüm dosyalar başarıyla yüklendiyse, dosya bilgilerini döndür
            if (results.TrueForAll(r => r.Equals(true)))
                return datas;

            // Hata durumunda null döndür
            return null;
        }



    }
}