using GenPhoto.Infrastructure;

namespace GenPhoto.ViewModels;

internal class ImageListItem : IListItem
{
    public Guid Id { get; init; }
    public string Title { get; init; } = "";

    public bool Filter(string word)
    {
        return Title.Contains(word, StringComparison.OrdinalIgnoreCase);
    }
}
