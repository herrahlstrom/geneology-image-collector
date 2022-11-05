using GenPhoto.Data;
using GenPhoto.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace GenPhoto;

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

        return new ImageDisplayViewModel(imageLoader)
        {
            Id = entity.Id,
            Name = entity.Title,
            Notes = entity.Notes,
            Path = entity.Path,
            FullPath = Path.Combine(settings.RootPath, entity.Path),
            Persons = persons.ConvertAll(x => new KeyValuePair<Guid, string>(x.Id, x.Name))
        };
    }

    public PersonDisplayViewModel GetPersonDisplayViewModel(Guid id)
    {
        throw new NotImplementedException();
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