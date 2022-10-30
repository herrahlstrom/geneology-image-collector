using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618

namespace MyGen.Data.Models;

public class FamilyLifeStory
{
    public Family Family { get; set; }

    [Column("family")]
    public int FamilyId { get; set; }

    public LifeStory LifeStory { get; set; }

    [Column("life_story")]
    public int LifeStoryId { get; set; }
}