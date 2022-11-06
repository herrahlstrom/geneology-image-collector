namespace GenPhoto.Data.Models;

public class Image
{
    public Guid TypeId { get; set; }

    public DateTime Added { get; set; }

    public Guid Id { get; set; }

    public bool Missing { get; set; }

    public string Notes { get; set; } = "";

    public string Path { get; set; } = "";

    public string Title { get; set; } = "";
}