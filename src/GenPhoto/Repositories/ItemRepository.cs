using GenPhoto.Data;
using GenPhoto.Extensions;
using GenPhoto.Models;
using GenPhoto.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace GenPhoto.Repositories
{
    internal class ItemRepository
    {
        public ItemRepository(AppState appState, IDbContextFactory<AppDbContext> dbFactory, AppSettings settings)
        {
            AppState = appState;
            DbFactory = dbFactory;
            Settings = settings;
        }

        public AppState AppState { get; }

        public IDbContextFactory<AppDbContext> DbFactory { get; }

        public AppSettings Settings { get; }

        public async Task<ICollection<ImageViewModel>> GetItemsAsync()
        {
            using var db = await DbFactory.CreateDbContextAsync();

            var images = await (from image in db.Images
                                orderby image.Title
                                select new
                                {
                                    image.Id,
                                    image.Title,
                                    image.Path
                                }).ToListAsync();

            var metaOnImages = await (from meta in db.ImageMeta
                                      where meta.Value != ""

                                      join metaKey in db.ImageMetaKey on meta.Key equals metaKey.Key into metaKeyJoin
                                      from metaKey in metaKeyJoin.DefaultIfEmpty()

                                      select new
                                      {
                                          meta.ImageId,
                                          meta.Key,
                                          DisplayKey = metaKey.DisplayText ?? meta.Key,
                                          meta.Value,
                                          Sort = (int?)metaKey.Sort ?? 999
                                      }).ToListAsync();

            var metaOnImagesDict = metaOnImages
                .GroupBy(x => x.ImageId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderBy(x=> x.Sort).Select(x => new MetaItem
                    {
                        Key = x.Key,
                        Value = x.Value,
                        DisplayKey = x.DisplayKey
                    }).ToList());

            var personInImages = await (from personImage in db.PersonImages
                                        join person in db.Persons on personImage.PersonId equals person.Id
                                        select new
                                        {
                                            personImage.ImageId,
                                            personImage.PersonId,
                                            person.Name
                                        }).ToListAsync();
            var personInImagesDict = personInImages
                .GroupBy(x => x.ImageId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(x => new ImagePersonItem { Id = x.PersonId, Name = x.Name }).ToList());

            var result = new List<ImageViewModel>(images.Count);

            foreach (var image in images)
            {
                MetaCollection meta = metaOnImagesDict.TryGetValue(image.Id, out var metaItems)
                    ? new MetaCollection(metaItems)
                    : MetaCollection.Empty;
                string? suggestedPath = meta.GetFilePath(image.Path);

                result.Add(new ImageViewModel()
                {
                    Id = image.Id,
                    Title = image.Title,
                    Path = image.Path,
                    FullPath = Path.Combine(Settings.RootPath, image.Path),
                    Persons = personInImagesDict.TryGetValue(image.Id, out var persons) ? persons : Array.Empty<ImagePersonItem>(),
                    Meta = meta,
                    SuggestedPath = suggestedPath,
                    MiniImage = null,
                    MidiImage = null,
                });
            }

            return result;
        }

        public async Task RenameImageFile(Guid id, string newPath)
        {
            using var db = await DbFactory.CreateDbContextAsync();

            var entity = await db.Images.FirstAsync(x => x.Id == id);

            var oldFullPath = Path.Combine(Settings.RootPath, entity.Path);

            entity.Path = newPath;

            var newFullPath = Path.Combine(Settings.RootPath, entity.Path);

            DirectoryInfo newDirectoryInfo = new FileInfo(newFullPath).Directory!;
            if (!newDirectoryInfo.Exists)
            {
                newDirectoryInfo.Create();
            }

            File.Move(oldFullPath, newFullPath);

            await db.SaveChangesAsync();
        }
    }
}