using System.ComponentModel.DataAnnotations.Schema;

namespace GenPhoto.Data.Models;

[Table("PersonImage")]
public class PersonImage
{
    [Column("image")]
    public Guid ImageId { get; set; }

    [Column("person")]
    public Guid PersonId { get; set; }
}