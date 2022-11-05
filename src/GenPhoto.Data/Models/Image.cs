using System.ComponentModel.DataAnnotations.Schema;

namespace GenPhoto.Data.Models;

public class Image
{
    [Column("type")]
    public Guid TypeId { get; set; }

    [Column("added")]
    public DateTime Added { get; set; }

    [Column("id")]
    public Guid Id { get; set; }

    [Column("missing")]
    public bool Missing { get; set; }

    [Column("notes")]
    public string Notes { get; set; } = "";

    [Column("path")]
    public string Path { get; set; } = "";

    [Column("title")]
    public string Title { get; set; } = "";
}