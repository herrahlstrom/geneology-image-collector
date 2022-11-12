﻿using GenPhoto.Data;
using GenPhoto.Infrastructure;
using GenPhoto.Infrastructure.ViewModels;
using GenPhoto.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GenPhoto.ViewModels;

internal interface IMainViewModel : ILoadableViewModel
{
    SearchViewModel<IListItem> Search { get; }
}

internal class MainViewModel : ViewModelBase, IMainViewModel
{
    private readonly IDbContextFactory<AppDbContext> m_dbFactory;

    public MainViewModel(AppState appState, IDbContextFactory<AppDbContext> dbFactory, DisplayViewModelRepository displayViewModelRepository)
    {
        m_dbFactory = dbFactory;

        DisplayItems = new HistoryHolder<IDisplayViewModel>();

        Search = new SearchViewModel<IListItem>()
        {
            SelectedItemCallback = (item) =>
            {
                switch (item)
                {
                    case PersonListItem person: appState.OpenPerson(person.Id); break;
                    case ImageListItem image: appState.OpenImage(image.Id); break;
                    default: throw new NotSupportedException();
                }
            }
        };

        appState.OpenImageRequest += async (_, id) => DisplayItems.Add(await displayViewModelRepository.GetImageDisplayViewModelAsync(id));
        appState.OpenPersonRequest += async (_, id) => DisplayItems.Add(await displayViewModelRepository.GetPersonDisplayViewModel(id));
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

        items.AddRange(await (
            from img in db.Images
            join imgType in db.ImageTypes on img.TypeId equals imgType.Id
            orderby img.Title
            select new ImageListItem
            {
                Id = img.Id,
                Title = img.Title,
                TypeKey = imgType.Key
            }).ToListAsync().ConfigureAwait(true));

        items.AddRange(await db.Persons.OrderBy(x => x.Name).Select(x => new PersonListItem
        {
            Id = x.Id,
            Name = x.Name
        }).ToListAsync().ConfigureAwait(true));

        Search.UpdateItems(items);
    }
}