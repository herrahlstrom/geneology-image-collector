using GenPhoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GenPhoto.Data;

public class AppDbContext : DbContext
{
   public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
   {
   }

   public DbSet<Image> Images { get; set; } = null!;
   public DbSet<ImageType> ImageTypes { get; set; } = null!;
   public DbSet<PersonImage> PersonImages { get; set; } = null!;
   public DbSet<Person> Persons { get; set; } = null!;

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      modelBuilder.Entity<Image>()
       .HasKey(x => x.Id);

      modelBuilder.Entity<Person>()
       .HasKey(x => x.Id);

      modelBuilder.Entity<PersonImage>()
       .HasKey(x => new { x.ImageId, x.PersonId });

      modelBuilder.Entity<ImageType>()
       .HasKey(x => x.Id);
   }
}