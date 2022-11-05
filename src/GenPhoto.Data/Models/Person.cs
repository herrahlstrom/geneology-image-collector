using System.ComponentModel.DataAnnotations.Schema;

namespace GenPhoto.Data.Models;

[Table("Person")]
public class Person
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";
}
