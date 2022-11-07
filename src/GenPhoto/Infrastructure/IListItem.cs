namespace GenPhoto.Infrastructure;

internal interface IListItem
{
    string SortKey { get; }

    bool Filter(string[] words) => words.All(Filter);

    bool Filter(string word);
}