﻿using GenPhoto.Extensions;
using GenPhoto.Helpers;
using GenPhoto.Infrastructure;
using GenPhoto.Models;
using GenPhoto.Repositories;
using GenPhoto.Shared;
using System.ComponentModel;
using System.Diagnostics;
using System.Printing;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GenPhoto.ViewModels
{
    public class ImageViewModel : ViewModelBase
    {
        private readonly List<MetaItemViewModel> _meta = new();
        private readonly List<ImagePersonViewModel> _persons = new();
        private readonly ItemRepository _repo;
        private readonly AppSettings _settings;
        private List<KeyValuePair<Guid, string>> _availablePersons = new List<KeyValuePair<Guid, string>>();
        private bool _editMode;
        private ImageSource? _midiImage;
        private ImageSource? _miniImage;
        private string _path = "";
        private string? _suggestedPath;
        private Guid? selectedAvailablePerson;
        private string availablePersonsFilter = "";

        public ImageViewModel(ItemRepository repo, AppSettings settings)
        {
            _repo = repo;
            _settings = settings;

            Persons = new ListCollectionView(_persons);

            Meta = new ListCollectionView(_meta)
            {
                Filter = (obj) => EditMode || obj is MetaItemViewModel { Value.Length: > 0 },
            };
            Meta.SortDescriptions.Add(new SortDescription(nameof(MetaItemViewModel.Sort), ListSortDirection.Ascending));

            AvailablePersons = new ListCollectionView(_availablePersons)
            {
                Filter = (obj) =>
                    string.IsNullOrEmpty(AvailablePersonsFilter) ||
                    obj is KeyValuePair<Guid, string> item && item.Value.Contains(AvailablePersonsFilter, StringComparison.OrdinalIgnoreCase)
            };

            AvailablePersons.SortDescriptions.Add(new SortDescription("Value", ListSortDirection.Ascending));

            OpenFileCommand = new RelayCommand(
                canExecute: () => FullPath.HasValue(),
                execute: () => Process.Start(new ProcessStartInfo("explorer", FullPath!)));

            AddPersonCommand = new RelayCommand(
                canExecute: () => selectedAvailablePerson.HasValue,
                execute: () =>
                {
                    if (selectedAvailablePerson is { } pId)
                    {
                        var name = _availablePersons.Where(x => x.Key == pId).Select(x => (string?)x.Value).FirstOrDefault();
                        _persons.Add(new ImagePersonViewModel() { Id = pId, Name = name ?? "", State = EntityState.Added });
                        Persons.Refresh();
                    }
                });

            RenameImageCommand = new RelayCommand(
                canExecute: () => SuggestedPath.HasValue() && SuggestedPath != Path,
                execute: async () => await repo.MoveImageFileToSuggestedPath(this));

            EditCommand = new RelayCommand(execute: async () => await BeginEdit());
            UndoCommand = new RelayCommand(execute: async () => await UndoChanges());
            SaveCommand = new RelayCommand(execute: async () => await SaveChanges());

            RemovePersonCommand = new RelayCommand<ImagePersonViewModel>(
                canExecute: (ImagePersonViewModel? p) => p?.State != EntityState.Deleted,
                execute: (ImagePersonViewModel? p) => { p!.State = EntityState.Deleted; });
        }

        public ListCollectionView AvailablePersons { get; }
        public string AvailablePersonsFilter
        {
            get => availablePersonsFilter;
            set
            {
                availablePersonsFilter = value;
                AvailablePersons.Refresh();
            }
        }
        public Guid? SelectedAvailablePerson
        {
            get => selectedAvailablePerson;
            set
            {
                selectedAvailablePerson = value;
                AddPersonCommand.Revaluate();
            }
        }
        public IRelayCommand EditCommand { get; }

        public bool EditMode
        {
            get { return _editMode; }
            set
            {
                SetProperty(ref _editMode, value);
                Meta.Refresh();
            }
        }

        public string FullPath => System.IO.Path.Combine(_settings.RootPath, Path);
        public Guid Id { get; init; }

        public ListCollectionView Meta { get; }

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

        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public ListCollectionView Persons { get; }
        public IRelayCommand RemovePersonCommand { get; }

        public IRelayCommand AddPersonCommand { get; }

        public IRelayCommand RenameImageCommand { get; }

        public IRelayCommand SaveCommand { get; }

        public string? SuggestedPath
        {
            get => _suggestedPath;
            set => SetProperty(ref _suggestedPath, value);
        }

        public string Title { get; init; } = "";

        public IRelayCommand UndoCommand { get; }

        public void AddMeta(IEnumerable<MetaItemViewModel> meta)
        {
            _meta.AddRange(meta);
            Meta.Refresh();

            SuggestedPath = _meta.GetFilePath(Path);
        }

        public void AddPersons(IEnumerable<ImagePersonViewModel> persons)
        {
            _persons.AddRange(persons);
            Persons.Refresh();
        }

        public async Task BeginEdit()
        {
            MidiImage ??= new BitmapImage(ImageHelper.GetImageDisplayPath(Id, FullPath, new(400, 400)));

            await Task.Delay(1);

            _meta.AddRange(from keyId in Enum.GetValues(typeof(ImageMetaKey)) as IList<int>
                           let key = Enum.GetName(typeof(ImageMetaKey), keyId)
                           where _meta.All(meta => meta.Key != key)
                           select new MetaItemViewModel(key, "", EntityState.Added));

            if (_availablePersons.Count == 0)
            {
                _availablePersons.AddRange(await _repo.GetAvailablePersons());
                AvailablePersons.Refresh();
            }

            EditMode = true;
        }

        public bool HasMetaValue(string metaKey, string metaValue)
        {
            return _meta.Any(x => x.Key == metaKey && x.Value.Equals(metaValue, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasPerson(Guid id) => _persons.Any(x => x.Id == id);

        public bool IsMatch(string w)
        {
            return Title.Contains(w, StringComparison.CurrentCultureIgnoreCase) ||
                Path.Contains(w, StringComparison.CurrentCultureIgnoreCase) ||
                _persons.Any(x => x.IsMatch(w)) ||
                _meta.IsMatch(w);
        }

        public async Task SaveChanges()
        {
            EditMode = false;

            // Update meta
            var tmpMetaList = new List<MetaItemViewModel>(_meta);
            foreach (var item in tmpMetaList)
            {
                switch (item.State)
                {
                    case EntityState.Added when string.IsNullOrWhiteSpace(item.Value):
                    case EntityState.Unmodified:
                        continue;

                    case EntityState.Deleted:
                    case EntityState.Modified when string.IsNullOrWhiteSpace(item.Value):
                        await _repo.RemoveMetaOnImage(Id, item.Key);
                        item.State = EntityState.Unmodified;
                        break;

                    case EntityState.Added:
                    case EntityState.Modified:
                        await _repo.AddOrUpdateMetaOnImage(Id, item);
                        item.State = EntityState.Unmodified;
                        break;

                    default:
                        throw new NotSupportedException($"Invalid state: {item.State}");
                }
            }

            Meta.Refresh();

            // Update persons
            var tmpPersonList = new List<ImagePersonViewModel>(_persons);
            foreach (var p in tmpPersonList)
            {
                switch (p.State)
                {
                    case EntityState.Added:
                        await _repo.AddPersonToImage(Id, p.Id);
                        p.State = EntityState.Unmodified;
                        break;

                    case EntityState.Deleted:
                        await _repo.RemovePersonFromImage(Id, p.Id);
                        Persons.Remove(p);
                        break;

                    case EntityState.Unmodified:
                        continue;

                    default:
                        throw new NotSupportedException($"Invalid state: {p.State}");
                }
            }

            Persons.Refresh();

            if (_meta.GetFilePath(Path) is { } suggestedPath && suggestedPath != Path)
            {
                SuggestedPath = suggestedPath;
            }
            else
            {
                SuggestedPath = null;
            }
        }

        public async Task UndoChanges()
        {
            EditMode = false;

            await Task.Delay(1);
        }
    }
}