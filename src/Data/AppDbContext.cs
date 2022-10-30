using GeneologyImageCollector.Data.Models;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618

namespace GeneologyImageCollector.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Image> Images { get; set; }
    public DbSet<ImageType> ImageTypes { get; set; }
    public DbSet<PersonImage> PersonImages { get; set; }
    public DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Image>()
            .ToTable("Image").HasKey(x => x.Id);

        modelBuilder.Entity<Person>()
            .ToTable("Person").HasKey(x => x.Id);

        modelBuilder.Entity<PersonImage>()
            .ToTable("PersonImage").HasKey(x => new { x.ImageId, x.PersonId });
        
        modelBuilder.Entity<ImageType>()
            .ToTable("ImageType").HasKey(x => x.Id);
    }
}