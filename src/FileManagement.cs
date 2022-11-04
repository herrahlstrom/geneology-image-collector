using GeneologyImageCollector.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace GeneologyImageCollector
{
    internal class FileManagement
    {
        private readonly AppDbContext _db;
        private readonly AppSettings _settings;

        public FileManagement(AppSettings settings, AppDbContext db)
        {
            _settings = settings;
            _db = db;
        }

        public async Task FindNewFilesAsync()
        {
            var files = SearchFiles(new DirectoryInfo(_settings.RootPath), "");

            var dbFiles = (await _db.Images.Select(x => x.Path).ToListAsync()).ToHashSet();

            var newFiles = files.Where(x => !dbFiles.Contains(x)).ToList();
            if (newFiles.Count == 0)
            {
                return;
            }

            var defualtImageTypeId = await _db.ImageTypes.Where(x => x.Key == "").Select(x => x.Id).SingleAsync();

            foreach (var file in newFiles)
            {
                Data.Models.Image entity = new()
                {
                    Id = Guid.NewGuid(),
                    Added = DateTime.UtcNow,
                    Title = Path.GetFileName(file),
                    Path = file,
                    TypeId = defualtImageTypeId,
                    Notes = "",
                };
                _db.Images.Add(entity);
            }

            await _db.SaveChangesAsync();
        }

        public async Task FindMissingFilesAsync()
        {
            var files = SearchFiles(new DirectoryInfo(_settings.RootPath), "").ToHashSet();

            var dbFiles = await _db.Images.ToListAsync();

            var missingFiles = dbFiles.Where(x => files.Contains(x.Path)).ToList();
            if (missingFiles.Count == 0)
            {
                return;
            }

            foreach (var entiry in missingFiles)
            {
                entiry.Missing = true;
            }

            await _db.SaveChangesAsync();
        }

        private static List<string> SearchFiles(DirectoryInfo dir, string relativePath)
        {
            var result = new List<string>();

            foreach (var subDir in dir.GetDirectories())
            {
                if (subDir.Name.StartsWith("."))
                {
                    continue;
                }

                result.AddRange(SearchFiles(subDir, GetRelativeSubPath(subDir)));
            }

            foreach (var file in dir.GetFiles())
            {
                switch (file.Extension.ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                        result.Add(GetRelativeSubPath(file));
                        break;

                    default:
                        throw new NotSupportedException("Not supported file extension: " + file.Extension);
                }
            }

            return result;

            string GetRelativeSubPath(FileSystemInfo item)
            {
                return string.IsNullOrEmpty(relativePath)
                   ? item.Name
                   : $"{relativePath}\\{item.Name}";
            }
        }
    }
}