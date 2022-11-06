using GenPhoto.Helpers;
using GenPhoto.Infrastructure;
using GenPhoto.Infrastructure.ViewModels;
using System.Drawing;
using System.Threading;
using System.Windows.Media;

namespace GenPhoto.ViewModels;

internal class ImageDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    private static readonly SemaphoreSlim _loadSemaphore = new(1);
    private readonly ImageLoader imageLoader;

    private ImageSource? _image = null;
    private bool _loaded = false;

    public ImageDisplayViewModel(ImageLoader imageLoader)
    {
        this.imageLoader = imageLoader;

        OpenImageCommand = new RelayCommand(
            canExecute: () => OpenFileCommand.CanExecute(FullPath),
            execute: () => OpenFileCommand.Execute(FullPath));
    }

    public string FullPath { get; init; } = "";

    public Guid Id { get; init; }

    public ImageSource? Image
    {
        get => _image;
        private set => SetProperty(ref _image, value);
    }

    public string Name { get; set; } = "";
    public string Notes { get; set; } = "";
    public IRelayCommand OpenImageCommand { get; }
    public string Path { get; init; } = "";
    public ICollection<KeyValuePair<Guid, string>> Persons { get; init; } = Array.Empty<KeyValuePair<Guid, string>>();
    public ICollection<KeyValuePair<string, string>> Meta { get; init; } = Array.Empty<KeyValuePair<string, string>>();

    protected override async void LoadCommand_Execute()
    {
        if (_loaded)
        {
            return;
        }

        await _loadSemaphore.WaitAsync();
        try
        {
            if (_loaded)
            {
                return;
            }

            try
            {
                Size maxSize = new(1000, 1000);
                Image = imageLoader.GetImageSource(FullPath);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            _loaded = true;
        }
        finally
        {
            _loadSemaphore.Release();
        }
    }
}