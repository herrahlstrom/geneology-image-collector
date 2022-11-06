using GenPhoto.Data;
using GenPhoto.Models;
using GenPhoto.Parser;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace GenPhoto.Tools
{
    internal class Maintenance
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly AppSettings _settings;

        public Maintenance(IDbContextFactory<AppDbContext> dbFactory, AppSettings settings)
        {
            _dbFactory = dbFactory;
            _settings = settings;
        }

        public async Task FindMissingFilesAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var files = SearchFiles(new DirectoryInfo(_settings.RootPath), "").ToHashSet();

            var dbFiles = await db.Images.ToListAsync();

            var missingFiles = dbFiles.Where(x => files.Contains(x.Path)).ToList();
            if (missingFiles.Count == 0)
            {
                return;
            }

            foreach (var entiry in missingFiles)
            {
                entiry.Missing = true;
            }

            await db.SaveChangesAsync();
        }

        public async Task FindNewFilesAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var files = SearchFiles(new DirectoryInfo(_settings.RootPath), "");

            var dbFiles = (await db.Images.Select(x => x.Path).ToListAsync()).ToHashSet();

            var newFiles = files.Where(x => !dbFiles.Contains(x)).ToList();
            if (newFiles.Count == 0)
            {
                return;
            }

            var defualtImageTypeId = await db.ImageTypes.Where(x => x.Key == "").Select(x => x.Id).SingleAsync();

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
                db.Images.Add(entity);
            }

            await db.SaveChangesAsync();
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