using GenPhoto.Data;
using GenPhoto.Data.Models;
using GenPhoto.Models;
using GenPhoto.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace GenPhoto.Repositories
{
    public class ItemRepository
    {
        private readonly EntityRepositoryFactory m_entityRepository;

        public ItemRepository(AppState appState, EntityRepositoryFactory entityRepository, AppSettings settings)
        {
            AppState = appState;
            m_entityRepository = entityRepository;
            Settings = settings;
        }

        public AppState AppState { get; }
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
            IList<Image> images = await GetAllEntities<Image>();
            var personImages = await GetAllEntities<PersonImage>();
            var persons = await GetAllEntities<Person>();
            Dictionary<Guid, List<ImageMeta>> meta = (await GetAllEntities<ImageMeta>()).GroupBy(x => x.ImageId).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<string, ImageMetaKey> metaKeys = (await GetAllEntities<ImageMetaKey>()).ToDictionary(x => x.Key, x => x);

            var personsInImages = (
                from pi in personImages
                join p in persons on pi.PersonId equals p.Id
                select new
                {
                    pi.ImageId,
                    PersonId = p.Id,
                    PersonName = p.Name
                }).GroupBy(x => x.ImageId).ToDictionary(x => x.Key, x => x.Select(x => new { x.PersonId, x.PersonName }));

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

                if (personsInImages.TryGetValue(image.Id, out var personsInImage))
                {
                    model.AddPersons(from person in personsInImage
                                     select new ImagePersonViewModel()
                                     {
                                         Id = person.PersonId,
                                         Name = person.PersonName
                                     });
                }

                if (meta.TryGetValue(image.Id, out var metaEntities))
                {
                    model.AddMeta(from entity in metaEntities
                                  let tmp = metaKeys[entity.Key]
                                  orderby tmp?.Sort ?? 999
                                  select new MetaItemViewModel(entity.Key, entity.Value)
                                  {
                                      DisplayKey = tmp?.DisplayText ?? entity.Key
                                  });
                }

                result.Add(model);
            }

            return result;

            async Task<IList<TEntity>> GetAllEntities<TEntity>() where TEntity : class
            {
                using var imageRepo = m_entityRepository.Create<TEntity>();
                return await imageRepo.GetEntitiesAsync();
            }
        }

        public async Task MoveImageFileToSuggested(ImageViewModel model)
        {
            if (model.SuggestedPath is not { Length: > 0 } suggestedPath)
            {
                return;
            }

            using var repo = m_entityRepository.Create<Image>();

            await repo.UpdateEntityAsync((entity) =>
            {
                string fullPathFrom = Path.Combine(Settings.RootPath, entity.Path);
                string fullPathTo = Path.Combine(Settings.RootPath, suggestedPath);

                try
                {
                    File.Move(fullPathFrom, fullPathTo);
                }
                catch (DirectoryNotFoundException ex)
                {
                    new FileInfo(fullPathTo).Directory!.Create();
                    File.Move(fullPathFrom, fullPathTo);
                }
                model.Path = suggestedPath;
                model.SuggestedPath = null;

                entity.Path = suggestedPath;
            }, model.Id);
        }

        public async Task RemoveMetaOnImage(Guid imageId, string metaKey)
        {
            using var repo = m_entityRepository.Create<ImageMeta>();

            await repo.RemoveEntityAsync(ImageMeta.GetKey(imageId, metaKey));
        }

        public async Task RemovePersonFromImage(Guid imageId, Guid personId)
        {
            using var repo = m_entityRepository.Create<PersonImage>();

            await repo.RemoveEntityAsync(imageId, personId);
        }
    }
}