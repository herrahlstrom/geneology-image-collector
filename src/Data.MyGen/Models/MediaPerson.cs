using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618

namespace MyGen.Data.Models;

public class MediaPerson
{
    public Media Media { get; set; }

    [Column("media")]
    public int MediaId { get; set; }

    [Column("pos")]
    public int MediaPos { get; set; } = 99;

    public Person Person { get; set; }

    [Column("person")]
    public int PersonId { get; set; }
}