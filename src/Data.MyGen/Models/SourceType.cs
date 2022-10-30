using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618

namespace MyGen.Data.Models;

public class SourceType
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [MaxLength(50)]
    public string Name { get; set; }
}