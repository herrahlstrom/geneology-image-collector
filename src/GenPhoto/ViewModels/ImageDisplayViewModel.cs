using GenPhoto.Helpers;
using GenPhoto.Infrastructure;
using GenPhoto.Infrastructure.ViewModels;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GenPhoto.ViewModels;

internal class ImageDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    private static readonly SemaphoreSlim _loadSemaphore = new(1);

    private ImageSource? _image = null;
    private bool _loaded = false;

    public ImageDisplayViewModel()
    {
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

    public MetaCollection Meta { get; init; } = MetaCollection.Empty;
    public string Name { get; set; } = "";
    public string Notes { get; set; } = "";
    public IRelayCommand OpenImageCommand { get; }
    public string Path { get; init; } = "";
    public ICollection<KeyValuePair<Guid, string>> Persons { get; init; } = Array.Empty<KeyValuePair<Guid, string>>();

    protected override async void LoadCommand_Execute()
    {
        if (_loaded)
        {
            return;
        }

        await _loadSemaphore.WaitAsync();

        if (_loaded)
        {
            return;
        }

        try
        {
            Image = new BitmapImage(new Uri(FullPath));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            _loaded = true;
            _loadSemaphore.Release();
        }
    }
}