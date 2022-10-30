using System.ComponentModel.DataAnnotations.Schema;

namespace GeneologyImageCollector.Data.Models;

public class PersonImage
{
    [Column("image")]
    public Guid ImageId { get; set; }

    [Column("person")]
    public Guid PersonId { get; set; }
}