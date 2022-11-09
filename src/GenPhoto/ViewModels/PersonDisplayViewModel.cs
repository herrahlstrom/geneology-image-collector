using GenPhoto.Helpers;
using GenPhoto.Infrastructure;
using GenPhoto.Infrastructure.ViewModels;
using System.Windows.Media;

namespace GenPhoto.ViewModels;

internal class PersonDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    private readonly ImageLoader _imageLoader;
    private readonly Queue<PersonImageItemViewModel> _loadImageQueue;

    public PersonDisplayViewModel(ImageLoader imageLoader, Guid id, string name, IEnumerable<PersonImageItemViewModel> items)
    {
        _imageLoader = imageLoader;
        Id = id;
        Name = name;

        _loadImageQueue = new(items);
    }

    public SearchViewModel<PersonImageItemViewModel> FilteredImageViewModel { get; } = new() { AllItemsOnEmptyFilter = true };
    public Guid Id { get; }
    public string Name { get; }

    protected override void LoadCommand_Execute()
    {
        while (_loadImageQueue.TryDequeue(out var item))
        {
            item.Image ??= _imageLoader.GetImageSource(item.Id, item.FullPath, new(200, 200));
            FilteredImageViewModel.AddItem(item);
            FilteredImageViewModel.Items.Refresh();
        }
    }
}

internal class PersonImageItemViewModel : ViewModelBase, IListItem
{
    private ImageSource? _image = null;
    public required string FullPath { get; init; }
    public required Guid Id { get; init; }

    public ImageSource? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }

    public MetaCollection Meta { get; init; } = MetaCollection.Empty;
    public required string SortKey { get; init; }
    public required string Title { get; set; }

    public bool Filter(string word)
    {
        return string.IsNullOrEmpty(word) ||
            FullPath.Contains(word, StringComparison.OrdinalIgnoreCase) ||
            Title.Contains(word, StringComparison.OrdinalIgnoreCase) ||
            Meta.Any(x => x.Value.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}