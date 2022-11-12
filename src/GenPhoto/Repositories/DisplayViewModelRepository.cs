using GenPhoto.Data;
using GenPhoto.Extensions;
using GenPhoto.Helpers;
using GenPhoto.Models;
using GenPhoto.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace GenPhoto.Repositories;

internal class DisplayViewModelRepository
{
    private readonly IDbContextFactory<AppDbContext> dbFactory;
    private readonly AppSettings settings;

    public AppState AppState { get; }

    public DisplayViewModelRepository(AppState appState, IDbContextFactory<AppDbContext> dbFactory, AppSettings settings)
    {
        AppState = appState;
        this.dbFactory = dbFactory;
        this.settings = settings;
    }

    public async Task<ImageDisplayViewModel> GetImageDisplayViewModelAsync(Guid id)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var entity = await (from e in db.Images
                            where e.Id == id
                            select new
                            {
                                e.Id,
                                e.Title,
                                e.Path,
                                e.Notes
                            }).FirstAsync();

        var persons = await (from pi in db.PersonImages
                             where pi.ImageId == id
                             join p in db.Persons on pi.PersonId equals p.Id
                             select new
                             {
                                 p.Id,
                                 p.Name
                             }).ToListAsync();

        return new ImageDisplayViewModel()
        {
            Id = entity.Id,
            Name = entity.Title,
            Notes = entity.Notes,
            Path = entity.Path,
            FullPath = Path.Combine(settings.RootPath, entity.Path),
            Persons = persons.ConvertAll(x => new KeyValuePair<Guid, string>(x.Id, x.Name)),
            Meta = await GetImageMeta(db, id),
        };
    }

    public async Task<PersonDisplayViewModel> GetPersonDisplayViewModel(Guid id)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var entity = await (from e in db.Persons
                            where e.Id == id
                            select new
                            {
                                e.Id,
                                e.Name
                            }).FirstAsync();

        var images = await (from personImage in db.PersonImages
                            join image in db.Images on personImage.ImageId equals image.Id
                            where personImage.PersonId == id
                            select new
                            {
                                image.Id,
                                image.Title,
                                image.Path
                            }).ToListAsync();

        List<PersonImageItemViewModel> imageViewModels = new(images.Count);
        foreach (var img in images)
        {
            MetaCollection meta = await GetImageMeta(db, img.Id);

            imageViewModels.Add(new PersonImageItemViewModel()
            {
                Id = img.Id,
                FullPath = Path.Combine(settings.RootPath, img.Path),
                Title = img.Title,
                Meta = meta,
                SortKey = meta.GetSortKey()
            });
        }

        return new PersonDisplayViewModel(AppState,
            id: entity.Id,
            name: entity.Name,
            items: imageViewModels);
    }

    public async IAsyncEnumerable<Guid> GetRandomImageId(int count)
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var ids = await db.Images.Select(x => x.Id).ToListAsync();

        while (count > 0)
        {
            count--;
            yield return ids[Random.Shared.Next(0, ids.Count)];
        }
    }

    private static async Task<MetaCollection> GetImageMeta(AppDbContext db, Guid id)
    {
        List<MetaItem> meta = new();
        meta.AddRange(await (from imgMeta in db.ImageMeta
                             where imgMeta.ImageId == id
                             join metaKey in db.ImageMetaKey on imgMeta.Key equals metaKey.Key
                             orderby metaKey.Sort
                             select new MetaItem
                             {
                                 Key = imgMeta.Key,
                                 DisplayKey = metaKey.DisplayText,
                                 Value = imgMeta.Value
                             }).ToListAsync());
        meta.AddRange(await (from imgMeta in db.ImageMeta
                             where imgMeta.ImageId == id
                             where !db.ImageMetaKey.Any(x => x.Key == imgMeta.Key)
                             select new MetaItem
                             {
                                 Key = imgMeta.Key,
                                 DisplayKey = imgMeta.Key,
                                 Value = imgMeta.Value
                             }).ToListAsync());
        return new MetaCollection(meta);
    }
}