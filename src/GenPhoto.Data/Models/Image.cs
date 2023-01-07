namespace GenPhoto.Data.Models;

public class Image
{
    public DateTime Added { get; set; }
    public Guid Id { get; set; }
    public bool Missing { get; set; }
    public DateTime Modified { get; set; }
    public string Notes { get; set; } = "";
    public string Path { get; set; } = "";
    public int Size { get; set; }
    public string Title { get; set; } = "";
    public string FileCrc { get; set; } = "";
    public Guid TypeId { get; set; }
}
