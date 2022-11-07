using GenPhoto.Helpers;
using GenPhoto.Infrastructure;
using GenPhoto.Infrastructure.ViewModels;
using System.Windows.Media;

namespace GenPhoto.ViewModels;

internal class PersonDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    private readonly ImageLoader _imageLoader;
    private readonly Queue<PersonImageItemViewModel> _loadQueue = new();

    public PersonDisplayViewModel(ImageLoader imageLoader, Guid id, string name, IEnumerable<PersonImageItemViewModel> items)
    {
        _imageLoader = imageLoader;
        Id = id;
        Name = name;

        foreach (var item in items)
        {
            FilteredImageViewModel.AddItem(item);

            if (item.Image is null)
            {
                _loadQueue.Enqueue(item);
            }
        }

        FilteredImageViewModel.Items.Refresh();
    }

    public SearchViewModel<PersonImageItemViewModel> FilteredImageViewModel { get; } = new() { AllItemsOnEmptyFilter = true };
    public Guid Id { get; }
    public string Name { get; }

    protected override void LoadCommand_Execute()
    {
        foreach (var image in _loadQueue)
        {
            image.Image = _imageLoader.GetImageSource(image.Id, image.FullPath, new System.Drawing.Size(200, 200));
        }
    }
}

internal class PersonImageItemViewModel : ViewModelBase, IListItem
{
    private ImageSource? _image = null;
    public string FullPath { get; init; } = "";
    public Guid Id { get; init; }

    public ImageSource? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }

    public MetaCollection Meta { get; init; } = MetaCollection.Empty;
    public string Title { get; set; } = "";

    public string SortKey { get; init; } = "";

    public bool Filter(string word)
    {
        return string.IsNullOrEmpty(word) ||
            FullPath.Contains(word, StringComparison.OrdinalIgnoreCase) ||
            Title.Contains(word, StringComparison.OrdinalIgnoreCase) ||
            Meta.Any(x => x.Value.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}