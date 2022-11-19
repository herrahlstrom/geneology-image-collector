namespace GenPhoto.Shared;

public enum ImageMetaKey
{
    Image,
    Page,
    Reference,
    Repository,
    Volume,
    Year,
    Location
}

[Obsolete]
public static class ImageMetaKeys
{
    public const string Image = nameof(Image);
    public const string Page = nameof(Page);
    public const string Reference = nameof(Reference);
    public const string Repository = nameof(Repository);
    public const string Volume = nameof(Volume);
    public const string Year = nameof(Year);
    public const string Location = nameof(Location);
}
