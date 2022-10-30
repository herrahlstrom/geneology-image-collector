using System.ComponentModel.DataAnnotations.Schema;

namespace GeneologyImageCollector.Data.Models;

public class ImageType
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("key")]
    public string Key { get; set; } = "";

    [Column("name")]
    public string Name { get; set; } = "";
}
