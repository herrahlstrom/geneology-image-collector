using System.Windows.Media;

namespace GenPhoto.Infrastructure;

internal interface IListItem
{
    bool Filter(string[] words) => words.All(Filter);

    bool Filter(string word);
}
