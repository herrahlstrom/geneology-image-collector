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

    public MainViewModel(IDbContextFactory<AppDbContext> dbFactory, DisplayViewModelRepository displayViewModelRepository)
    {
        m_dbFactory = dbFactory;

        DisplayItems = new HistoryHolder<IDisplayViewModel>();

        Search = new SearchViewModel<IListItem>()
        {
           SelectedItemCallback = async (item) =>
           {
              DisplayItems.Add(item switch
              {
                 PersonListItem person => displayViewModelRepository.GetPersonDisplayViewModel(person.Id),
                 ImageListItem image => await displayViewModelRepository.GetImageDisplayViewModelAsync(image.Id),
                 _ => throw new NotSupportedException()
              });
           }
        };
    }

    public HistoryHolder<IDisplayViewModel> DisplayItems { get; }
    public SearchViewModel<IListItem> Search { get; }

    protected override async void LoadCommand_Execute()
    {
        await UpdateSearchItems();
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