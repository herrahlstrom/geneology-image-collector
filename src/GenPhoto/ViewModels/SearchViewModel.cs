﻿using GenPhoto.Infrastructure;
using System.ComponentModel;
using System.Windows.Data;

namespace GenPhoto.ViewModels;

internal class SearchViewModel<T> : ViewModelBase where T : IListItem
{
    private readonly List<T> m_items;
    private T? _selectedItem;

    private Action<T>? _selectedItemCallback;
    private string m_searchFilter = "";

    private string[] m_searchFilterArray = Array.Empty<string>();

    public SearchViewModel()
    {
        m_items = new List<T>();

        Items = new ListCollectionView(m_items)
        {
            Filter = FilterItems
        };

        Items.SortDescriptions.Add(new SortDescription(nameof(IListItem.SortKey), ListSortDirection.Ascending));

        PropertyChanged += OnPropertyChanged;
    }

    public bool AllItemsOnEmptyFilter { get; set; } = false;

    public string Filter
    {
        get => m_searchFilter;
        set => SetProperty(ref m_searchFilter, value);
    }

    public ListCollectionView Items { get; }

    public T? SelectedItem
    {
        get { return _selectedItem; }
        set { SetProperty(ref _selectedItem, value); }
    }

    public Action<T>? SelectedItemCallback
    {
        get { return _selectedItemCallback; }
        init { _selectedItemCallback = value; }
    }

    public void AddItem(T item)
    {
        m_items.Add(item);
    }

    public void UpdateItems(IEnumerable<T> items)
    {
        m_items.Clear();
        m_items.AddRange(items);

        Items.Refresh();
    }

    private bool FilterItems(object obj)
    {
        if (obj is not IListItem item)
        {
            return false;
        }

        if (m_searchFilterArray.Length > 0)
        {
            return item.Filter(m_searchFilterArray);
        }
        else if (AllItemsOnEmptyFilter)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Filter))
        {
            m_searchFilterArray = string.IsNullOrWhiteSpace(Filter)
                ? Array.Empty<string>()
                : Filter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            Items.Refresh();
        }

        if (e.PropertyName == nameof(SelectedItem) && SelectedItem is { } item)
        {
            SelectedItemCallback?.Invoke(item);
        }
    }
}