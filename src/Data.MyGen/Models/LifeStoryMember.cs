using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618

namespace MyGen.Data.Models;

public class LifeStoryMember
{
    /// <summary>
    ///     Only if override from LifeStory (parent)
    /// </summary>
    [Column("date")]
    [MaxLength(20)]
    public string? Date { get; set; }

    /// <summary>
    ///     Only if override from LifeStory (parent)
    /// </summary>
    [Column("date_end")]
    [MaxLength(20)]
    public string? EndDate { get; set; }

    public LifeStory LifeStory { get; set; }

    [Column("life_story")]
    public int LifeStoryId { get; set; }

    public Person Person { get; set; }

    [Column("person")]
    public int PersonId { get; set; }
}