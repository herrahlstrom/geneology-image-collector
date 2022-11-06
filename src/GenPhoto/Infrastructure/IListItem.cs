using System.Windows.Media;

namespace GenPhoto.Infrastructure;

internal interface IListItem
{
    bool Filter(string[] words) => words.All(Filter);

    bool Filter(string word);
}

internal class ImageListItem : IListItem
{
    public Guid Id { get; init; }
    public string Title { get; init; } = "";

    public bool Filter(string word)
    {
        return Title.Contains(word, StringComparison.OrdinalIgnoreCase);
    }
}

internal class PersonListItem : IListItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";

    public bool Filter(string word)
    {
        return Name.Contains(word, StringComparison.OrdinalIgnoreCase);
    }
}