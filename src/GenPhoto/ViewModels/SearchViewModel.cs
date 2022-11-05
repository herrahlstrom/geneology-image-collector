using System.Windows.Data;

namespace GenPhoto.ViewModels;

internal class SearchViewModel<T> : ViewModelBase
{
    private readonly List<T> m_items;
    private string m_searchFilter = "";
    private string[] m_searchFilterArray = Array.Empty<string>();

    public SearchViewModel()
    {
        m_items = new List<T>();

        Items = new ListCollectionView(m_items)
        {
            Filter = FilterItems
        };

        PropertyChanged += OnPropertyChanged;
    }

    public string Filter
    {
        get => m_searchFilter;
        set => SetProperty(ref m_searchFilter, value);
    }

    public ListCollectionView Items { get; }

    public void UpdateItems(IEnumerable<T> items)
    {
        m_items.Clear();
        m_items.AddRange(items);

        Items.Refresh();
    }

    private bool FilterItems(object obj)
    {
        return obj is IListItem item && m_searchFilterArray.Length > 0 && item.Filter(m_searchFilterArray);
    }

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Filter))
        {
            m_searchFilterArray = string.IsNullOrWhiteSpace(Filter)
                ? Array.Empty<string>()
                : Filter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            Items.Refresh();
        }
    }
}