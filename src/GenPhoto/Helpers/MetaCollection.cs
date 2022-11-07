using GenPhoto.Shared;
using System.Collections;

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

    public string? Repository => this.Where(x => x.Key == ImageMetaKeys.Repository).Select(x => x.Value).FirstOrDefault();
    public string? Volume => this.Where(x => x.Key == ImageMetaKeys.Volume).Select(x => x.Value).FirstOrDefault();
    public string? Year => this.Where(x => x.Key == ImageMetaKeys.Year).Select(x => x.Value).FirstOrDefault();

    public IEnumerator<MetaItem> GetEnumerator()
    {
        return m_list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_list.GetEnumerator();
    }
}

class MetaItem
{
    public string Key { get; init; } = "";
    public string DisplayKey { get; init; }= "";
    public string Value { get; init; }= "";
}