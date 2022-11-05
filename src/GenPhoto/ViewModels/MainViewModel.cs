using GenPhoto.Data;
using GenPhoto.Infrastructure;
using GenPhoto.Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace GenPhoto.ViewModels;

internal interface IMainViewModel : ILoadableViewModel
{
    SearchViewModel<IListItem> Search { get; }
}

internal class MainViewModel : ViewModelBase, IMainViewModel
{
    private readonly IDbContextFactory<AppDbContext> m_dbFactory;
    private readonly DisplayViewModelRepository displayViewModelRepository;

    public MainViewModel(IDbContextFactory<AppDbContext> dbFactory, DisplayViewModelRepository displayViewModelRepository)
    {
        m_dbFactory = dbFactory;
        this.displayViewModelRepository = displayViewModelRepository;
        DisplayItems = new HistoryHolder<IDisplayViewModel>();

        Search = new SearchViewModel<IListItem>();
    }

    public HistoryHolder<IDisplayViewModel> DisplayItems { get; }
    public SearchViewModel<IListItem> Search { get; }

    protected override async void LoadCommand_Execute()
    {
        await UpdateSearchItems();

        DisplayItems.Add(new PersonDisplayViewModel
        {
            Name = $"Person {DateTime.UtcNow}"
        });

        await foreach(var randomId in displayViewModelRepository.GetRandomImageId(10))
        {
            DisplayItems.Add(await displayViewModelRepository.GetImageDisplayViewModelAsync(randomId));
        }

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