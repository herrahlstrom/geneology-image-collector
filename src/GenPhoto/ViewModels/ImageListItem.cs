using GenPhoto.Infrastructure;

namespace GenPhoto.ViewModels;

internal class ImageListItem : IListItem
{
    public Guid Id { get; init; }
    public string SortKey => Title;
    public string Title { get; init; } = "";
    public required string TypeKey {get;init;}

    public bool Filter(string word)
    {
        return Title.Contains(word, StringComparison.OrdinalIgnoreCase);
    }
}