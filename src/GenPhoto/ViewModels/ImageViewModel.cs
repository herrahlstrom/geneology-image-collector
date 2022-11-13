using GenPhoto.Infrastructure;
using GenPhoto.Models;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace GenPhoto.ViewModels
{
    public class ImageViewModel : ViewModelBase
    {
        private ImageSource? _midiImage = null;
        private ImageSource? _miniImage = null;
        private string? _suggestedPath = null;

        public ImageViewModel()
        {
            OpenFileCommand = new RelayCommand(
                canExecute: () => File.Exists(FullPath),
                execute: () => Process.Start(new ProcessStartInfo("explorer", FullPath!)));
        }

        public required string FullPath { get; init; }

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
        
        public required string Path { get; init; }
        
        public required ICollection<ImagePersonItem> Persons { get; init; }
        
        public string? SuggestedPath
        {
            get { return _suggestedPath; }
            set { SetProperty(ref _suggestedPath, value); }
        }

        public required string Title { get; init; }

        public bool IsMatch(string w)
        {
            return Title.Contains(w, StringComparison.CurrentCultureIgnoreCase) ||
                Path.Contains(w, StringComparison.CurrentCultureIgnoreCase) ||
                Persons.Any(x => x.IsMatch(w)) ||
                Meta.IsMatch(w);
        }
    }
}