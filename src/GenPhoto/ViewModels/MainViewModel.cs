using GenPhoto.Helpers;
using GenPhoto.Infrastructure;
using GenPhoto.Models;
using GenPhoto.Repositories;
using GenPhoto.Shared;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Timer = System.Timers.Timer;

namespace GenPhoto.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly List<FilterOption> _filterOptions = new List<FilterOption>();
        private readonly Api _itemRepo;
        private readonly List<ImageViewModel> _items = new List<ImageViewModel>();
        private Timer _inputTimer;
        private ConcurrentQueue<ImageViewModel> _loadImageQueue = new();
        private string m_searchFilter = "";
        private string[] m_searchFilterArray = Array.Empty<string>();
        static object _loadImageQueueLock = new object();

        public MainViewModel(Api itemRepo)
        {
            _inputTimer = new()
            {
                Interval = TimeSpan.FromMilliseconds(400).TotalMilliseconds,
                AutoReset = false
            };
            _inputTimer.Elapsed += (_, _) => Filter(true);

            Items = new ListCollectionView(_items)
            {
                Filter = (obj) => obj is ImageViewModel item &&
                    m_searchFilterArray.Length > 0 &&
                    m_searchFilterArray.All(item.IsMatch) &&
                    _filterOptions.All(filterOption => FilterByOptions(item, filterOption))
            };

            FilterOptions = new ListCollectionView(_filterOptions)
            {
                Filter = (obj) => obj is FilterOption { Options.Count: > 0 }
            };

            LoadCommand = new RelayCommand(LoadCommand_Execute);

            _itemRepo = itemRepo;
            PropertyChanged += OnPropertyChanged;
        }

        public ListCollectionView FilterOptions { get; }

        public string FilterText
        {
            get => m_searchFilter;
            set => SetProperty(ref m_searchFilter, value);
        }

        public ListCollectionView Items { get; }
        public IRelayCommand LoadCommand { get; }

        public string Title => string.IsNullOrWhiteSpace(FilterText) ? "Gen Photo" : "Gen Photo | " + FilterText;

        protected async void LoadCommand_Execute()
        {
            _items.AddRange(await _itemRepo.GetItemsAsync());
            Items.Refresh();
        }

        private static bool FilterByOptions(ImageViewModel item, FilterOption filterOption)
        {
            if (string.IsNullOrEmpty(filterOption.SelectedOption))
            {
                return true;
            }
            else if (filterOption.Key == "person" && Guid.TryParse(filterOption.SelectedOption, out var personId))
            {
                if (personId == Guid.Empty || item.HasPerson(personId))
                {
                    return true;
                }
            }
            else if (filterOption.Key.StartsWith("meta."))
            {
                var key = filterOption.Key[5..];
                if (item.HasMetaValue(key, filterOption.SelectedOption))
                {
                    return true;
                }
            }

            return false;
        }

        private void Filter(bool rebuildOptions)
        {
            Items.Dispatcher.Invoke(Items.Refresh);

            var filteredItems = Items.OfType<ImageViewModel>().ToList();

            if (rebuildOptions)
            {
                RebuildFilterOptions(filteredItems);
            }

            foreach (var item in filteredItems.Where(x => x.MiniImage is null).Take(20))
            {
                _loadImageQueue.Enqueue(item);
            }

            if (!_loadImageQueue.IsEmpty)
            {
                new Thread(new ThreadStart(ProcessImageQueue)).Start();
            }
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterText))
            {
                _inputTimer.Enabled = false;
                m_searchFilterArray = string.IsNullOrWhiteSpace(FilterText)
                    ? Array.Empty<string>()
                    : FilterText.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                _inputTimer.Start();
            }
        }

        private void ProcessImageQueue()
        {
            while (_loadImageQueue.TryDequeue(out var item))
            {
                lock (_loadImageQueueLock)
                {
                    if (item.MiniImage != null)
                    {
                        continue;
                    }

                    try
                    {
                        var uri = ImageHelper.GetImageDisplayPath(item.Id, item.FullPath, new(200, 200));
                        Items.Dispatcher.Invoke(() => item.MiniImage = new BitmapImage(uri));
                    }
                    catch (ExternalException) { }
                    catch (FileNotFoundException) { }
                }
            }
        }

        private void RebuildFilterOptions(List<ImageViewModel> filteredItems)
        {
            FilterOptions.Dispatcher.Invoke(_filterOptions.Clear);

            // Filter option | Person
            _filterOptions.Add(new FilterOption()
            {
                Key = "person",
                Name = "Person",
                Options = filteredItems
                        .SelectMany(x => x.Persons.OfType<ImagePersonViewModel>().Select(y => new { y.Id, y.Name }))
                        .Append(new { Id = Guid.Empty, Name = "" })
                        .Distinct()
                        .OrderBy(x => x.Name)
                        .Select(x => new KeyValuePair<string, string>(x.Id.ToString(), x.Name))
                        .ToList(),
                SelectedOptionChangedCallback = () => Filter(false)
            });

            // Filter option | Repository
            _filterOptions.Add(new FilterOption()
            {
                Key = $"meta.{ImageMetaKey.Repository}",
                Name = "Arkiv",
                Options = filteredItems
                    .SelectMany(x => x.Meta.OfType<MetaItemViewModel>())
                    .Where(x => x.Key == nameof(ImageMetaKey.Repository))
                    .Select(x => x.Value)
                    .Append("")
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new KeyValuePair<string, string>(x, x))
                    .ToList(),
                SelectedOptionChangedCallback = () => Filter(false)
            });

            // Filter option | Volume
            _filterOptions.Add(new FilterOption()
            {
                Key = $"meta.{ImageMetaKey.Volume}",
                Name = "Volym",
                Options = filteredItems
                    .SelectMany(x => x.Meta.OfType<MetaItemViewModel>())
                    .Where(x => x.Key == nameof(ImageMetaKey.Volume))
                    .Select(x => x.Value)
                    .Append("")
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new KeyValuePair<string, string>(x, x))
                    .ToList(),
                SelectedOptionChangedCallback = () => Filter(false)
            });

            FilterOptions.Dispatcher.Invoke(FilterOptions.Refresh);

        }
    }
}