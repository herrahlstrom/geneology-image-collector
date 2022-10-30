using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618

namespace MyGen.Data.Models;

public class Media
{
    [Column("id")]
    public int Id { get; set; }
    [Column("path")]
    public string Path { get; set; }
    public ICollection<MediaPerson> Persons { get; set; }
}