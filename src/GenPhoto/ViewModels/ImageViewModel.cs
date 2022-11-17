using GenPhoto.Extensions;
using GenPhoto.Helpers;
using GenPhoto.Infrastructure;
using GenPhoto.Models;
using GenPhoto.Repositories;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GenPhoto.ViewModels
{
    public class ImageViewModel : ViewModelBase
    {
        private readonly ItemRepository _repo;
        private readonly AppSettings _settings;
        private bool _editMode;
        private ImageSource? _midiImage;
        private ImageSource? _miniImage;
        private string _path = "";
        private string? _suggestedPath;

        public ImageViewModel(ItemRepository repo, AppSettings settings)
        {
            _repo = repo;
            _settings = settings;

            OpenFileCommand = new RelayCommand(
                canExecute: () => FullPath.HasValue(),
                execute: () => Process.Start(new ProcessStartInfo("explorer", FullPath!)));

            RenameImageCommand = new RelayCommand(
                canExecute: () => SuggestedPath.HasValue() && SuggestedPath != Path,
                execute: async () => await repo.MoveImageFileToSuggested(this));

            EditCommand = new RelayCommand(execute: async () => await BeginEdit());
            UndoCommand = new RelayCommand(execute: async () => await UndoChanges());
            SaveCommand = new RelayCommand(execute: async () => await SaveChanges());

            RemovePersonCommand = new RelayCommand<ImagePersonViewModel>(
                canExecute: (ImagePersonViewModel? p) => p?.Deleted == false,
                execute: (ImagePersonViewModel? p) => { p!.Deleted = true; });
        }

        public IRelayCommand EditCommand { get; }

        public bool EditMode
        {
            get { return _editMode; }
            set { SetProperty(ref _editMode, value); }
        }

        public string FullPath => System.IO.Path.Combine(_settings.RootPath, Path);

        public required Guid Id { get; init; }

        public required MetaCollection Meta { get; init; }

        public ImageSource? MidiImage
        {
            get => _midiImage;
            set => SetProperty(ref _midiImage, value);
        }

        public ImageSource? MiniImage
        {
            get => _miniImage;
            set => SetProperty(ref _miniImage, value);
        }

        public IRelayCommand OpenFileCommand { get; }

        public required string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public IRelayCommand RemovePersonCommand { get; }
        public required IList<ImagePersonViewModel> Persons { get; init; }

        public IRelayCommand RenameImageCommand { get; }

        public IRelayCommand SaveCommand { get; }

        public string? SuggestedPath
        {
            get => _suggestedPath;
            set => SetProperty(ref _suggestedPath, value);
        }

        public required string Title { get; init; }

        public IRelayCommand UndoCommand { get; }

        public async Task BeginEdit()
        {
            MidiImage ??= new BitmapImage(ImageHelper.GetImageDisplayPath(Id, FullPath, new(400, 400)));

            await Task.Delay(1);

            EditMode = true;
        }

        public bool IsMatch(string w)
        {
            return Title.Contains(w, StringComparison.CurrentCultureIgnoreCase) ||
                Path.Contains(w, StringComparison.CurrentCultureIgnoreCase) ||
                Persons.Any(x => x.IsMatch(w)) ||
                Meta.IsMatch(w);
        }

        public async Task SaveChanges()
        {
            EditMode = false;

            // ToDo: Update meta fields

            // Remove deleted persons
            for(int i=Persons.Count -1; i>=0; i--)
            {
                if (Persons[i].Deleted)
                {
                    await _repo.RemovePersonFromImage(Id, Persons[i].Id);
                    Persons.RemoveAt(i);
                }
            }

            // ToDo: Add new persons

            OnPropertyChanged(nameof(Persons));

            await Task.Delay(1);
        }

        public async Task UndoChanges()
        {
            EditMode = false;

            await Task.Delay(1);
        }
    }
}