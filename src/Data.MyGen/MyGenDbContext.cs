using MyGen.Data.Models;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618

namespace MyGen.Data;

public class MyGenDbContext : DbContext
{
    public MyGenDbContext(DbContextOptions<MyGenDbContext> options) : base(options) { }

    public DbSet<Family> Families { get; set; }

    public DbSet<FamilyLifeStory> FamilyLifeStories { get; set; }

    public DbSet<FamilyMember> FamilyMembers { get; set; }

    public DbSet<FamilyMemberType> FamilyMemberTypes { get; set; }

    public DbSet<LifeStory> LifeStories { get; set; }

    public DbSet<LifeStoryMember> LifeStoryMembers { get; set; }

    public DbSet<LifeStorySource> LifeStorySources { get; set; }

    public DbSet<LifeStoryType> LifeStoryTypes { get; set; }

    public DbSet<Media> Media { get; set; }

    public DbSet<MediaPerson> MediaPersons { get; set; }

    public DbSet<Person> Persons { get; set; }

    public DbSet<Source> Sources { get; set; }

    public DbSet<SourceType> SourceTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Family>(b =>
        {
            b.ToTable("Family").HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();
            b.HasMany(x => x.Members).WithOne(x => x.Family).HasForeignKey(x => x.FamilyId);
        });

        modelBuilder.Entity<FamilyMember>(b => { b.ToTable("FamilyMember").HasKey(x => new { x.FamilyId, x.PersonId }); });

        modelBuilder.Entity<FamilyMemberType>(b => { b.ToTable("FamilyMemberType").HasKey(x => x.Id); });

        modelBuilder.Entity<LifeStory>(b =>
        {
            b.ToTable("LifeStory").HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<LifeStoryMember>(b =>
        {
            b.ToTable("LifeStoryMember").HasKey(x => new { x.LifeStoryId, x.PersonId });

            b.HasOne(x => x.Person).WithMany(x => x.LifeStories).HasForeignKey(x => x.PersonId);
            b.HasOne(x => x.LifeStory).WithMany(x => x.Members).HasForeignKey(x => x.LifeStoryId);
        });

        modelBuilder.Entity<FamilyLifeStory>(b =>
        {
            b.ToTable("FamilyLifeStory").HasKey(x => new { x.LifeStoryId, x.FamilyId });

            b.HasOne(x => x.Family).WithMany(x => x.LifeStories).HasForeignKey(x => x.FamilyId);
            b.HasOne(x => x.LifeStory).WithMany(x => x.Families).HasForeignKey(x => x.LifeStoryId);
        });

        modelBuilder.Entity<LifeStorySource>(b => { b.ToTable("LifeStorySource").HasKey(x => new { x.LifeStoryId, x.SourceId }); });

        modelBuilder.Entity<LifeStoryType>(b => { b.ToTable("LifeStoryType").HasKey(x => x.Id); });

        modelBuilder.Entity<Media>(b =>
        {
            b.ToTable("Media").HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<MediaPerson>(b =>
        {
            b.ToTable("MediaPerson").HasKey(x => new { x.MediaId, x.PersonId });

            b.HasOne(x => x.Media).WithMany(x => x.Persons).HasForeignKey(x => x.MediaId);
            b.HasOne(x => x.Person).WithMany(x => x.Media).HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<Person>(b =>
        {
            b.ToTable("Person").HasKey(x => x.Id);

            b.HasMany(x => x.Families).WithOne(x => x.Person).HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<Source>(b =>
        {
            b.ToTable("Source").HasKey(x => new { x.Id });
            b.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<SourceType>(b => { b.ToTable("SourceType").HasKey(x => x.Id); });
    }
}