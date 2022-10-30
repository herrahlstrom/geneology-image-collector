using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

#pragma warning disable CS8618

namespace MyGen.Data.Models;

[DebuggerDisplay("{Id} | {ReferenceId} | {Name} | {Url}")]
public class Source
{
    [Column("id")]
    public int Id { get; set; }

    [Column("img")]
    [MaxLength(200)]
    public string? ImagePath { get; set; }

    public ICollection<LifeStorySource> LifeStories { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Column("notes")]
    public string Notes { get; set; } = "";

    [Column("page")]
    [MaxLength(20)]
    public string? Page { get; set; }

    [Column("ref")]
    [MaxLength(20)]
    public string? ReferenceId { get; set; }

    [Column("repo")]
    [MaxLength(150)]
    public string? Repository { get; set; }

    public SourceType Type { get; set; }

    [Column("type")]
    public int TypeId { get; set; }

    [Column("url")]
    [MaxLength(200)]
    public string? Url { get; set; }

    [Column("volume")]
    [MaxLength(100)]
    public string? Volume { get; set; }
}