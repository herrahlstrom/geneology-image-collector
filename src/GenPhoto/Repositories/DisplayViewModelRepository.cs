using GenPhoto.Data;
using GenPhoto.Helpers;
using GenPhoto.Models;
using GenPhoto.Shared;
using GenPhoto.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace GenPhoto.Repositories;

internal class DisplayViewModelRepository
{
    private readonly IDbContextFactory<AppDbContext> dbFactory;
    private readonly ImageLoader imageLoader;
    private readonly AppSettings settings;

    public DisplayViewModelRepository(IDbContextFactory<AppDbContext> dbFactory, ImageLoader imageLoader, AppSettings settings)
    {
        this.dbFactory = dbFactory;
        this.imageLoader = imageLoader;
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

        var meta = await (from imgMeta in db.ImageMeta
                             where imgMeta.ImageId == id
                             select new
                             {
                                 imgMeta.Key,
                                 imgMeta.Value
                             }).ToListAsync();

        return new ImageDisplayViewModel(imageLoader)
        {
            Id = entity.Id,
            Name = entity.Title,
            Notes = entity.Notes,
            Path = entity.Path,
            FullPath = Path.Combine(settings.RootPath, entity.Path),
            Persons = persons.ConvertAll(x => new KeyValuePair<Guid, string>(x.Id, x.Name)),
            Meta = meta.Select(x=> new
            {
                Key = GetMetaDisplayKey(x.Key),
                x.Value,
                Sort = GetMetaSortValue(x.Key)
            }).OrderBy(x=> x.Sort).Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList(),
        };

        int GetMetaSortValue(string metaKey)
        {
            return metaKey switch
            {
                null => 0,
                "" => 0,
                
                ImageMetaKeys.Repository => 1,
                ImageMetaKeys.Volume => 2,

                ImageMetaKeys.Year => 3,
                ImageMetaKeys.Image => 4,
                ImageMetaKeys.Page => 5,
                ImageMetaKeys.Location => 6,
                ImageMetaKeys.Reference => 7,

                _ => int.MaxValue
            };
        }

        string GetMetaDisplayKey(string metaKey)
        {
            return metaKey switch
            {
                ImageMetaKeys.Repository => "Arkiv",
                ImageMetaKeys.Volume => "Volym",

                ImageMetaKeys.Year => "År",
                ImageMetaKeys.Image => "Bild",
                ImageMetaKeys.Page => "Sida",
                ImageMetaKeys.Location => "PLats",
                ImageMetaKeys.Reference => "Referens",

                _ => metaKey
            };
        }
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

        return new PersonDisplayViewModel()
        {
            Id = entity.Id,
            Name = entity.Name
        };
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
}