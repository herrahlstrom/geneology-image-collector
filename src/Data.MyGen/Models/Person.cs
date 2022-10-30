using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618

namespace MyGen.Data.Models;

public class Person
{
    public ICollection<FamilyMember> Families { get; set; }

    [Column("f_name")]
    [MaxLength(100)]
    public string Firstname { get; set; }

    [Column("id")]
    public int Id { get; set; }

    [Column("l_name")]
    [MaxLength(100)]
    public string Lastname { get; set; }

    public ICollection<LifeStoryMember> LifeStories { get; set; }

    public ICollection<MediaPerson> Media { get; set; }

    [Column("notes")]
    public string Notes { get; set; }

    [Column("profession")]
    [MaxLength(50)]
    public string Profession { get; set; }

    [Column("sex")]
    [MaxLength(1)]
    public string Sex { get; set; } = "";
}