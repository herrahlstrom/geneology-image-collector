using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618

namespace MyGen.Data.Models;

public class LifeStory
{
    [Column("date")]
    [MaxLength(20)]
    public string? Date { get; set; }

    [Column("date_end")]
    [MaxLength(20)]
    public string? EndDate { get; set; }

    public ICollection<FamilyLifeStory> Families { get; set; }

    [Column("id")]
    public int Id { get; set; }

    [Column("location")]
    [MaxLength(200)]
    public string? Location { get; set; }

    public ICollection<LifeStoryMember> Members { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public string? Name { get; set; }

    [Column("notes")]
    public string Notes { get; set; } = "";

    public ICollection<LifeStorySource> Sources { get; set; }

    public LifeStoryType Type { get; set; }

    [Column("type")]
    public int TypeId { get; set; }
}