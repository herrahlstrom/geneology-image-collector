using GeneologyImageCollector.Data;
using GeneologyImageCollector.Data.Models;
using Microsoft.EntityFrameworkCore;
using MyGen.Data;
using System.IO;

namespace GeneologyImageCollector;

internal class MyGenImporter
{
    private readonly AppSettings _settings;
    private AppDbContext _db;
    private MyGenDbContext _myGen;
    private Dictionary<int, Guid> _persons = new();

    private const int FamilyHusbandMember = 1;
    private const int FamilyWifeMember = 2;

    public MyGenImporter(AppDbContext db, MyGenDbContext myGen, AppSettings settings)
    {
        _db = db;
        _myGen = myGen;
        _settings = settings;
    }

    public async Task Import()
    {
        await ImportTypes();
        await ImportPortraits();
        await ImportSourceImages();

        await _db.SaveChangesAsync();
    }

    private void AddPersonsToImage(Guid imageId, IEnumerable<int> personIds)
    {
        foreach (var pId in personIds)
        {
            if (!_persons.TryGetValue(pId, out Guid personId))
            {
                var name = _myGen.Persons
                    .Where(x => x.Id == pId)
                    .Select(x => x.Firstname + " " + x.Lastname)
                    .First().Replace("*", "");
                var personEntity = new Person { Id = Guid.NewGuid(), Name = name };
                _db.Persons.Add(personEntity);

                personId = personEntity.Id;
                _persons.Add(pId, personId);
            }

            _db.PersonImages.Add(new PersonImage { ImageId = imageId, PersonId = personId });
        }
    }

    private async Task ImportPortraits()
    {
        var media = await _myGen.Media.Where(m => m.Path != null && m.Path != "").ToListAsync();
        var typeId = _db.ImageTypes.Where(x => x.Key == "portrait").Select(x => x.Id).Single();

        foreach (var m in media)
        {
            bool exists = File.Exists(Path.Combine(_settings.RootPath, "Foton", m.Path));

            Image imgEntity = new Image
            {
                Id = Guid.NewGuid(),
                TypeId = typeId,
                Title = m.Path,
                Notes = "",
                Added = DateTime.UtcNow,
                Missing = !exists,
                Path = $"Foton\\{m.Path}",
            };

            _db.Images.Add(imgEntity);

            List<int> personIds = await _myGen.MediaPersons.Where(x => x.MediaId == m.Id).Select(x => x.PersonId).ToListAsync();
            AddPersonsToImage(imgEntity.Id, personIds);
        }
    }

    private async Task ImportSourceImages()
    {
        var sources = await _myGen.Sources.Where(m => m.ImagePath != null && m.ImagePath != "").ToListAsync();
        var typeId = _db.ImageTypes.Where(x => x.Key == "source").Select(x => x.Id).Single();

        foreach (var s in sources)
        {
            bool exists = File.Exists(Path.Combine(_settings.RootPath, "Källor", s.ImagePath!));

            Image imgEntity = new Image
            {
                Id = Guid.NewGuid(),
                TypeId = typeId,
                Title = s.Name,
                Notes = s.Notes,
                Added = DateTime.UtcNow,
                Missing = !exists,
                Path = $"Källor\\{s.ImagePath}",
            };

            _db.Images.Add(imgEntity);

            var lifeStoryIds = await _myGen.LifeStorySources
                .Where(x => x.SourceId == s.Id)
                .Select(x => x.LifeStoryId).ToListAsync();

            List<int> personIds = await _myGen.LifeStoryMembers.Where(x => lifeStoryIds.Contains(x.LifeStoryId)).Select(x => x.PersonId).ToListAsync();

            var familyIds = await _myGen.FamilyLifeStories.Where(x => lifeStoryIds.Contains(x.LifeStoryId)).Select(x => x.FamilyId).ToListAsync();
            if (familyIds.Count > 0)
            {
                personIds.AddRange(await _myGen.FamilyMembers
                    .Where(x => familyIds.Contains(x.FamilyId))
                    .Where(x => x.MemberTypeId == FamilyHusbandMember || x.MemberTypeId == FamilyWifeMember)
                    .Select(x => x.PersonId)
                    .ToListAsync());
            }

            AddPersonsToImage(imgEntity.Id, personIds.Distinct());
        }
    }

    private async Task ImportTypes()
    {
        _db.ImageTypes.Add(new ImageType
        {
            Id = Guid.NewGuid(),
            Key = "",
            Name = "Övrig"
        });

        _db.ImageTypes.Add(new ImageType
        {
            Id = Guid.NewGuid(),
            Key = "portrait",
            Name = "Porträtt"
        });

        _db.ImageTypes.Add(new ImageType
        {
            Id = Guid.NewGuid(),
            Key = "source",
            Name = "Källa"
        });

        await _db.SaveChangesAsync();
    }
}