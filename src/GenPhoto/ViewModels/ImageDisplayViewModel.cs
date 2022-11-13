using GenPhoto.Extensions;
using GenPhoto.Helpers;
using GenPhoto.Infrastructure;
using GenPhoto.Infrastructure.ViewModels;
using GenPhoto.Repositories;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GenPhoto.ViewModels;

internal class ImageDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    private static readonly SemaphoreSlim _loadSemaphore = new(1);

    private ImageSource? _image = null;
    private bool _loaded = false;
    private string? _suggestedPath = null;
    private string path = "";

    public ImageDisplayViewModel(DisplayViewModelRepository repo)
    {
        OpenImageCommand = new RelayCommand(
            canExecute: () => OpenFileCommand.CanExecute(FullPath),
            execute: () => OpenFileCommand.Execute(FullPath));

        RenameImageCommand = new RelayCommand(
            canExecute: () => !string.IsNullOrEmpty(SuggestedPath) && SuggestedPath != Path,
            execute: async () =>
            {
                await repo.RenameImageFile(Id, SuggestedPath!);
                Path = SuggestedPath!;
                SuggestedPath = null;
            });
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

    public string Path { get => path; set => SetProperty(ref path, value); }

    public ICollection<KeyValuePair<Guid, string>> Persons { get; init; } = Array.Empty<KeyValuePair<Guid, string>>();

    public IRelayCommand RenameImageCommand { get; }

    public string? SuggestedPath
    {
        get { return _suggestedPath; }
        set { SetProperty(ref _suggestedPath, value); }
    }

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

        if (Meta.TryGetFilePath(Path, out var suggestedPath))
        {
            SuggestedPath = suggestedPath;
        }
        else
        {
            SuggestedPath = null;
        }

        try
        {
            Image = new BitmapImage(ImageHelper.GetImageDisplayPath(Id, FullPath, new(800, 800)));
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