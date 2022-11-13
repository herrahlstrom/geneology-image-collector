using GenPhoto.Shared;
using System.Collections;
using System.Diagnostics;

namespace GenPhoto.Helpers;

internal class MetaCollection : IReadOnlyCollection<MetaItem>
{
    private List<MetaItem> m_list = new();

    public MetaCollection(IEnumerable<MetaItem> items)
    {
        m_list.AddRange(items);
    }

    public static MetaCollection Empty { get; } = new MetaCollection(Enumerable.Empty<MetaItem>());
    public int Count => m_list.Count;

    public string? Image => GetValue(ImageMetaKeys.Image);
    public string? Location => GetValue(ImageMetaKeys.Location);
    public string? Page => GetValue(ImageMetaKeys.Page);
    public string? Reference => GetValue(ImageMetaKeys.Reference);
    public string? Repository => GetValue(ImageMetaKeys.Repository);
    public string? Volume => GetValue(ImageMetaKeys.Volume);
    public string? Year => GetValue(ImageMetaKeys.Year);

    public IEnumerator<MetaItem> GetEnumerator()
    {
        return m_list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_list.GetEnumerator();
    }

    public string? GetValue(string k)
    {
        return this.Where(x => x.Key == k).Select(x => x.Value).FirstOrDefault();
    }
}

[DebuggerDisplay("{Key} ({DisplayKey}) {Value}")]
internal class MetaItem
{
    public string DisplayKey { get; init; } = "";
    public string Key { get; init; } = "";
    public string Value { get; init; } = "";
}