using GenPhoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GenPhoto.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext()
    { }

    public DbSet<ImageMeta> ImageMeta { get; set; } = null!;
    public DbSet<Image> Images { get; set; } = null!;
    public DbSet<ImageType> ImageTypes { get; set; } = null!;
    public DbSet<PersonImage> PersonImages { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Image>(entity =>
        {
            entity
                .ToTable("Image")
                .HasKey(x => x.Id);

            entity.Property(exp => exp.Id).HasColumnName("id");
            entity.Property(exp => exp.TypeId).HasColumnName("type");
            entity.Property(exp => exp.Added).HasColumnName("added");
            entity.Property(exp => exp.Modified).HasColumnName("modified");
            entity.Property(exp => exp.Missing).HasColumnName("missing");
            entity.Property(exp => exp.Path).HasColumnName("path");
            entity.Property(exp => exp.Title).HasColumnName("title");
            entity.Property(exp => exp.Notes).HasColumnName("notes");
        });

        modelBuilder.Entity<ImageMeta>(entity =>
        {
            entity
                .ToTable("ImageMeta")
                .HasKey(x => new { x.ImageId, x.Key });

            entity.Property(exp => exp.ImageId).HasColumnName("image");
            entity.Property(exp => exp.Key).HasColumnName("key");
            entity.Property(exp => exp.Value).HasColumnName("value");
            entity.Property(exp => exp.Modified).HasColumnName("modified");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity
                .ToTable("Person")
                .HasKey(x => x.Id);

            entity.Property(exp => exp.Id).HasColumnName("id");
            entity.Property(exp => exp.Name).HasColumnName("name");
        });

        modelBuilder.Entity<PersonImage>(entity =>
        {
            entity
                .ToTable("PersonImage")
                .HasKey(x => new { x.ImageId, x.PersonId });

            entity.Property(exp => exp.ImageId).HasColumnName("image");
            entity.Property(exp => exp.PersonId).HasColumnName("person");
        });

        modelBuilder.Entity<ImageType>(entity =>
        {
            entity
                .ToTable("ImageType")
                .HasKey(x => x.Id);

            entity.Property(exp => exp.Id).HasColumnName("id");
            entity.Property(exp => exp.Key).HasColumnName("key");
            entity.Property(exp => exp.Name).HasColumnName("name");
        });
    }
}