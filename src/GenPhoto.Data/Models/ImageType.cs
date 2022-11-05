using System.ComponentModel.DataAnnotations.Schema;

namespace GenPhoto.Data.Models;

[Table("ImageType")]
public class ImageType
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("key")]
    public string Key { get; set; } = "";

    [Column("name")]
    public string Name { get; set; } = "";
}
