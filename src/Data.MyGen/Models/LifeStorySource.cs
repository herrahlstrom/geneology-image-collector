using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618

namespace MyGen.Data.Models;

public class LifeStorySource
{
    public LifeStory LifeStory { get; set; }

    [Column("life_story")]
    public int LifeStoryId { get; set; }

    public Source Source { get; set; }

    [Column("source")]
    public int SourceId { get; set; }
}