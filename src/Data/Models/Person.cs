using System.ComponentModel.DataAnnotations.Schema;

namespace GeneologyImageCollector.Data.Models;

public class Person
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";
}
