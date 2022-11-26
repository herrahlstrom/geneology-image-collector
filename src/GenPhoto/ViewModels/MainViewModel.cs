using GenPhoto.Extensions;
using GenPhoto.Helpers;
using GenPhoto.Infrastructure;
using GenPhoto.Repositories;
using GenPhoto.Tools;
using System.Collections.Concurrent;
using System.Diagnostics;
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

    DateTimeOffset m_itemsEndOfLife = DateTimeOffset.MinValue;
    private ConcurrentQueue<ImageViewModel> m_loadImageQueue = new();
    ImageSearcher m_searcher;
    private string m_statusText = "";

    private System.Timers.Timer m_statusTextClearTimer;

    public MainViewModel(Api itemRepo, Maintenance maintenance)
    {
        m_searcher = new ImageSearcher();

        Items = new ListCollectionView(m_items) { Filter = FilterItem };

        FilterOptions = new ListCollectionView(m_filterOptions)
        {
            Filter = (obj) => obj is FilterOption { } option && option.Options.Any(x => x.Key != "")
        };

        m_inputTimer = new() { AutoReset = false };
        m_inputTimer.Elapsed += async (_, _) => await OnFilterChangedAsync();

        LoadCommand = new RelayCommand(LoadCommand_Execute);

        SearchNewFilesCommand = new RelayCommand(async () =>
        {
            var sw = Stopwatch.StartNew();
            int result = await maintenance.FindNewFilesAsync();
            sw.Stop();
            SetStatusText($"Sökning av nya bilder slutförd. {result} nya bilder, {sw.ElapsedMilliseconds} ms.", TimeSpan.FromSeconds(7));
        });
        DetectMissingFilesCommand = new RelayCommand(async () =>
        {
            var sw = Stopwatch.StartNew();
            (int missing, int refound) = await maintenance.DetectMissingFilesAsync();
            sw.Stop();
            SetStatusText($"Sökning av saknader bilder slutförd. {missing} bilder saknade, {refound} bilder återfunna, {sw.ElapsedMilliseconds} ms.", TimeSpan.FromSeconds(7));
        });

        m_itemRepo = itemRepo;

        m_statusTextClearTimer = new System.Timers.Timer() { AutoReset = false };
        m_statusTextClearTimer.Elapsed += (_, _) => { StatusText = ""; };
    }

    private bool FilterItem(object obj)
    {
        if (obj is not ImageViewModel item)
        {
            return false;
        }

        return m_searcher.FilterItem(item, m_filterOptions);
    }
    private void LoadCommand_Execute()
    {
    }

    private async Task OnFilterChangedAsync()
    {
        m_searcher.BeforeFilter();

        if (m_itemsEndOfLife < DateTimeOffset.Now)
        {
            await UpdateItems();
        }

        RefreshItems(refreshOptions: true);
    }

    private void ProcessImageQueue()
    {
        while (m_loadImageQueue.TryDequeue(out var item))
        {
            if (item.MiniImage != null || item.FileMissing)
            {
                continue;
            }

            lock (m_loadImageQueueLock)
            {
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

    private void SetStatusText(string text, TimeSpan? autoClear = null)
    {
        m_statusTextClearTimer.Enabled = false;
        StatusText = text;
        if (autoClear is TimeSpan ts)
        {
            m_statusTextClearTimer.Interval = ts.TotalMilliseconds;
            m_statusTextClearTimer.Start();
        }
    }

    private async Task UpdateItems()
    {
        var items = await m_itemRepo.GetItemsAsync();
        m_items.Clear();
        m_items.AddRange(items);
        m_itemsEndOfLife = DateTimeOffset.Now + TimeSpan.FromMinutes(5);
    }

    public IRelayCommand DetectMissingFilesCommand { get; }

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
    public IRelayCommand SearchNewFilesCommand { get; }

    public string StatusText { get => m_statusText; set => SetProperty(ref m_statusText, value); }

    public string Title => string.IsNullOrWhiteSpace(FilterText) ? "Gen Photo" : "Gen Photo | " + FilterText;
}
