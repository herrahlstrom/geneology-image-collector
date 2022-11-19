using GenPhoto.Data;
using GenPhoto.Data.Models;
using GenPhoto.Extensions;
using GenPhoto.Models;
using GenPhoto.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace GenPhoto.Repositories
{
    public class ItemRepository
    {
        private readonly EntityRepositoryFactory m_entityRepository;

        public ItemRepository(AppState appState, IDbContextFactory<AppDbContext> dbFactory, EntityRepositoryFactory entityRepository, AppSettings settings)
        {
            AppState = appState;
            DbFactory = dbFactory;
            m_entityRepository = entityRepository;
            Settings = settings;
        }

        public AppState AppState { get; }

        public IDbContextFactory<AppDbContext> DbFactory { get; }

        public AppSettings Settings { get; }

        public async Task AddOrUpdateMetaOnImage(Guid imageId, KeyValuePair<string, string> meta)
        {
            using var repo = m_entityRepository.Create<ImageMeta>();

            await repo.AddOrUpdateEntityAsync(
                keyValues: new object[] { imageId, meta.Key },
                addAction: () => new ImageMeta
                {
                    ImageId = imageId,
                    Key = meta.Key,
                    Value = meta.Value,
                    Modified = DateTime.UtcNow
                },
                updateAction: (entity) =>
                {
                    entity.Value = meta.Value;
                    entity.Modified = DateTime.UtcNow;
                });
        }
        public async Task RemoveMetaOnImage(Guid imageId, string metaKey)
        {
            using var repo = m_entityRepository.Create<ImageMeta>();

            await repo.RemoveEntityAsync(ImageMeta.GetKey(imageId, metaKey));
        }

        public async Task AddPersonToImage(Guid imageId, Guid personId)
        {
            using var repo = m_entityRepository.Create<PersonImage>();

            await repo.AddEntityAsync(new PersonImage()
            {
                ImageId = imageId,
                PersonId = personId
            });
        }
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
                    x => x.OrderBy(x => x.Sort).Select(x => new MetaItemViewModel(x.Key, x.Value) { DisplayKey = x.DisplayKey }).ToList());

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
                    x => x.Select(x => new ImagePersonViewModel { Id = x.PersonId, Name = x.Name }).ToList());

            var result = new List<ImageViewModel>(images.Count);

            foreach (var image in images)
            {
                var model = new ImageViewModel(this, Settings)
                {
                    Id = image.Id,
                    Title = image.Title,
                    Path = image.Path,
                    SuggestedPath = null,
                    MiniImage = null,
                    MidiImage = null,
                };

                if(personInImagesDict.TryGetValue(image.Id, out var persons))
                {
                    model.AddPersons(persons);
                }

                if(metaOnImagesDict.TryGetValue(image.Id, out var metaItems))
                {
                    model.AddMeta(metaItems);
                    model.SuggestedPath = metaItems.GetFilePath(image.Path);
                }

                result.Add(model);
            }

            return result;
        }

        public async Task MoveImageFileToSuggested(ImageViewModel model)
        {
            if (model.SuggestedPath is { Length: > 0 } suggestedPath)
            {
                using var db = await DbFactory.CreateDbContextAsync();

                var entity = await db.Images.FirstAsync(x => x.Id == model.Id);

                var oldFullPath = Path.Combine(Settings.RootPath, entity.Path);

                entity.Path = suggestedPath;

                var newFullPath = Path.Combine(Settings.RootPath, entity.Path);

                DirectoryInfo newDirectoryInfo = new FileInfo(newFullPath).Directory!;
                if (!newDirectoryInfo.Exists)
                {
                    newDirectoryInfo.Create();
                }

                File.Move(oldFullPath, newFullPath);

                await db.SaveChangesAsync();

                model.Path = suggestedPath;
                model.SuggestedPath = null;
            }
        }

        public async Task RemovePersonFromImage(Guid imageId, Guid personId)
        {
            using var repo = m_entityRepository.Create<PersonImage>();

            await repo.RemoveEntityAsync(imageId, personId);
        }
    }
}