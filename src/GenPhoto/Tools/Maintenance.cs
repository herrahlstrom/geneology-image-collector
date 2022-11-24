using GenPhoto.Data;
using GenPhoto.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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

        public async Task DetectMissingFilesAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var files = SearchFiles(new DirectoryInfo(_settings.RootPath), "").ToHashSet();

            var dbFiles = await db.Images.ToListAsync();

            int changes = 0;

            foreach (var entity in dbFiles)
            {
                bool missing = !files.Contains(entity.Path);

                if (missing && !entity.Missing || !missing && entity.Missing)
                {
                    entity.Missing = missing;
                    changes++;
                }
            }
            if (changes == 0)
            {
                return;
            }

            if (changes > 1)
            {
                Debugger.Break();
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
                    Size = file.Length,
                };
                db.Images.Add(entity);
            }

            await db.SaveChangesAsync();
        }

        public async Task OneTimeFix()
        {
            using var db = await _dbFactory.CreateDbContextAsync();


            var miss = await db.Images.Where(x => x.Size == 0).ToListAsync();
            var paths = miss.Select(x => x.Path).ToList();

            foreach (var entry in miss)
            {
                var fullPath = System.IO.Path.Combine(_settings.RootPath, entry.Path);
                var fi = new FileInfo(fullPath);
                if (fi.Exists)
                {
                    entry.Size = (int)fi.Length;
                }
            }
            await db.SaveChangesAsync();
        }
    }
}