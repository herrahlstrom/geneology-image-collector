using GenPhoto.Extensions;
using GenPhoto.Helpers;
using GenPhoto.Infrastructure;
using GenPhoto.Repositories;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace GenPhoto.ViewModels;

internal class MainViewModel : ViewModelBase
{
    static readonly object m_loadImageQueueLock = new object();
    private readonly List<FilterOption> m_filterOptions = new List<FilterOption>();

    private readonly System.Timers.Timer m_inputTimer;
    private readonly Api m_itemRepo;
    private readonly List<ImageViewModel> m_items = new List<ImageViewModel>();
    private ConcurrentQueue<ImageViewModel> m_loadImageQueue = new();
    ImageSearcher m_searcher;

    public MainViewModel(Api itemRepo)
    {
        m_searcher = new ImageSearcher();

        Items = new ListCollectionView(m_items) { Filter = FilterItem };

        FilterOptions = new ListCollectionView(m_filterOptions)
        {
            Filter = (obj) => obj is FilterOption { } option && option.Options.Any(x => x.Key != "")
        };

        m_inputTimer = new() { AutoReset = false };
        m_inputTimer.Elapsed += (_, _) =>
        {
            m_searcher.BeforeFilter();
            RefreshItems(refreshOptions: true);
        };

        LoadCommand = new RelayCommand(LoadCommand_Execute);

        m_itemRepo = itemRepo;
    }

    private bool FilterItem(object obj)
    {
        if (obj is not ImageViewModel item)
        {
            return false;
        }

        return m_searcher.FilterItem(item, m_filterOptions);
    }
    private async void LoadCommand_Execute()
    {
        var items = await m_itemRepo.GetItemsAsync();
        m_items.AddRange(items);
        Items.Refresh();
    }

    private void ProcessImageQueue()
    {
        while (m_loadImageQueue.TryDequeue(out var item))
        {
            lock (m_loadImageQueueLock)
            {
                if (item.MiniImage != null || item.FileMissing)
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

    private void RefreshItems(bool refreshOptions)
    {
        Items.Dispatcher.Invoke(Items.Refresh);

        List<ImageViewModel> filteredItems = Items.OfType<ImageViewModel>().ToList();

        if (refreshOptions)
        {
            FilterOptions.Dispatcher.Invoke(
                () =>
                {
                    var options = m_searcher.BuildFilterOptions(
                        filteredItems,
                        () => RefreshItems(refreshOptions: false));
                    m_filterOptions.Clear();
                    m_filterOptions.AddRange(options);
                    FilterOptions.Refresh();
                });
        }

        m_loadImageQueue.EnqueueRange(filteredItems.Where(x => x.MiniImage is null).Take(20));

        if (!m_loadImageQueue.IsEmpty)
        {
            new Thread(new ThreadStart(ProcessImageQueue)).Start();
        }
    }

    public ListCollectionView FilterOptions { get; }
    public string FilterText
    {
        get => m_searcher.FilterText;
        set
        {
            m_searcher.FilterText = value;

            if (value?.Length > 4)
            {
                m_inputTimer.Interval = TimeSpan.FromMilliseconds(250).TotalMilliseconds;
            }
            else
            {
                m_inputTimer.Interval = TimeSpan.FromMilliseconds(750).TotalMilliseconds;
            }
            m_inputTimer.Start();
        }
    }
    public ListCollectionView Items { get; }
    public IRelayCommand LoadCommand { get; }

    public string Title => string.IsNullOrWhiteSpace(FilterText) ? "Gen Photo" : "Gen Photo | " + FilterText;
}
