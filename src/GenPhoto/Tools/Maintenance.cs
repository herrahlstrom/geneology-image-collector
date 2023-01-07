using DamienG.Security.Cryptography;
using GenPhoto.Data;
using GenPhoto.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace GenPhoto.Tools
{
    internal class Maintenance
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly AppSettings _settings;
        readonly ILogger<Maintenance> m_logger;

        public Maintenance(IDbContextFactory<AppDbContext> dbFactory, AppSettings settings, ILogger<Maintenance> logger)
        {
            m_logger = logger;
            _dbFactory = dbFactory;
            _settings = settings;
        }

        private string GetFileCrc(string filepath)
        {
            var crc32 = new Crc32();
            var hash = new StringBuilder(32);

            using (var fs = File.Open(filepath, FileMode.Open))
            {
                foreach (byte b in crc32.ComputeHash(fs))
                {
                    hash.AppendFormat("{0:x2}", b);
                }
            }

            return hash.ToString();
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

        public async Task<(int missing, int refound)> DetectMissingFilesAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var files = SearchFiles(new DirectoryInfo(_settings.RootPath), "").ToHashSet();

            var dbFiles = await db.Images.ToListAsync();

            int missingResult = 0;
            int refoundResult = 0;

            foreach (var entity in dbFiles)
            {
                bool missing = !files.Contains(entity.Path);

                if (missing && !entity.Missing)
                {
                    entity.Missing = missing;
                    missingResult++;
                }
                else if (!missing && entity.Missing)
                {
                    entity.Missing = missing;
                    refoundResult++;
                }
            }

            if (missingResult > 0 || refoundResult > 0)
            {
                Debugger.Break();
            }

            await db.SaveChangesAsync();

            return (missingResult, refoundResult);
        }

        public async Task<int> FindNewFilesAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var files = SearchFiles(new DirectoryInfo(_settings.RootPath), "");

            var dbFiles = (await db.Images.Select(x => x.Path).ToListAsync()).ToHashSet();

            var newFiles = files.Where(x => !dbFiles.Contains(x)).ToList();
            if (newFiles.Count == 0)
            {
                return 0;
            }

            var defualtImageTypeId = await db.ImageTypes.Where(x => x.Key == "").Select(x => x.Id).SingleAsync();

            foreach (var file in newFiles)
            {
                string hash = GetFileCrc(file);
                int size = (int)new FileInfo(file).Length;

                if (await db.Images.Where(x => x.FileCrc == hash).FirstOrDefaultAsync() is { } existsingFile)
                {
                    if (existsingFile.Missing)
                    {
                        m_logger.LogWarning("Skip import file {0}, but update missing image in database due its hash ({1}) is the same", file, hash);
                        existsingFile.Path = file;
                        existsingFile.Missing = false;
                        existsingFile.Modified = DateTime.UtcNow;
                    }
                    else
                    {
                        m_logger.LogWarning("Skip import file {0}, due its hash ({1}) alreaddy exists in database", file, hash);
                        continue;
                    }
                }

                Data.Models.Image entity = new()
                {
                    Id = Guid.NewGuid(),
                    Added = DateTime.UtcNow,
                    Title = Path.GetFileName(file),
                    Path = file,
                    TypeId = defualtImageTypeId,
                    Notes = "",
                    Size = size,
                    FileCrc = hash
                };
                db.Images.Add(entity);
            }

            int result = await db.SaveChangesAsync();

            return result;
        }

        public async Task OneTimeFix()
        {
            await Task.Delay(1);
            return;

            using var db = await _dbFactory.CreateDbContextAsync();

            var miss = await db.Images.Where(x => x.FileCrc == "").ToListAsync();
            if (miss.Count == 0)
            {
                return;
            }

            var hasher = new Crc32();

            foreach (var entry in miss)
            {
                var fullPath = Path.Combine(_settings.RootPath, entry.Path);
                entry.FileCrc = GetFileCrc(fullPath);
            }
            await db.SaveChangesAsync();
        }
    }
}