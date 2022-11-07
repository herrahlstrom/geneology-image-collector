namespace GenPhoto.Data.Models;

public class ImageMeta
{
    public Guid ImageId { get; set; }

    public DateTime Modified { get; set; }

    public string Key { get; set; } = "";

    public string Value { get; set; } = "";
}
