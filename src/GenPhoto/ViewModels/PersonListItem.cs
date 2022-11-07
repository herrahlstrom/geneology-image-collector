using GenPhoto.Infrastructure;

namespace GenPhoto.ViewModels;

internal class PersonListItem : IListItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";

    public string SortKey => Name;

    public bool Filter(string word)
    {
        return Name.Contains(word, StringComparison.OrdinalIgnoreCase);
    }
}