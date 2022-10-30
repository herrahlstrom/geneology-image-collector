using GeneologyImageCollector.Data;
using GeneologyImageCollector.Infrastructure;
using GeneologyImageCollector.Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace GeneologyImageCollector.ViewModels;

internal interface IMainViewModel : ILoadableViewModel
{
    SearchViewModel<IListItem> Search { get; }
}

internal class MainViewModel : ViewModelBase, IMainViewModel
{
    private readonly IDbContextFactory<AppDbContext> m_dbFactory;

    public MainViewModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        m_dbFactory = dbFactory;
        DisplayItems = new HistoryHolder<IDisplayViewModel>();

        LoadCommand = new RelayCommand(LoadCommand_Execute);

        Search = new SearchViewModel<IListItem>();
    }

    public HistoryHolder<IDisplayViewModel> DisplayItems { get; }
    public ICommand LoadCommand { get; }
    public SearchViewModel<IListItem> Search { get; }

    private async void LoadCommand_Execute()
    {
        await UpdateSearchItems();

        DisplayItems.Add(new PersonDisplayViewModel
        {
            Name = $"Person {DateTime.UtcNow}"
        });

        DisplayItems.Add(new ImageDisplayViewModel
        {
            Title = $"Bild {DateTime.UtcNow}"
        });

        DisplayItems.Add(new PersonDisplayViewModel
        {
            Name = $"Person {DateTime.UtcNow}"
        });
    }

    private async Task UpdateSearchItems()
    {
        var db = await m_dbFactory.CreateDbContextAsync();

        var items = new List<IListItem>();

        items.AddRange(await db.Images.OrderBy(x => x.Title).Select(x => new ImageListItem
        {
            Id = x.Id,
            Title = x.Title
        }).ToListAsync().ConfigureAwait(true));

        items.AddRange(await db.Persons.OrderBy(x => x.Name).Select(x => new PersonListItem
        {
            Id = x.Id,
            Name = x.Name
        }).ToListAsync().ConfigureAwait(true));

        Search.UpdateItems(items);
    }
}