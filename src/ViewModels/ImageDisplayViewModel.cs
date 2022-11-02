﻿using System.Drawing;
using System.Threading;
using System.Windows.Media;

namespace GeneologyImageCollector.ViewModels;

internal class ImageDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    private static readonly SemaphoreSlim _loadSemaphore = new(1);
    private readonly ImageLoader imageLoader;

    private ImageSource? _image = null;
    private bool _loaded = false;

    public ImageDisplayViewModel(ImageLoader imageLoader)
    {
        this.imageLoader = imageLoader;
    }

    public string FullPath { get; init; } = "";
    public Guid Id { get; init; }

    public ImageSource? Image
    {
        get => _image;
        private set => SetProperty(ref _image, value);
    }

    public string Notes { get; set; } = "";
    public string Path { get; init; } = "";
    public ICollection<KeyValuePair<Guid, string>> Persons { get; init; } = Array.Empty<KeyValuePair<Guid, string>>();
    public string Name { get; set; } = "";


    protected override async void LoadCommand_Execute()
    {
        if (_loaded)
        {
            return;
        }

        await _loadSemaphore.WaitAsync();
        try
        {
            if (_loaded)
            {
                return;
            }

            try
            {
                Size maxSize = new(1000, 1000);
                Image = imageLoader.GetImageSource(FullPath);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            _loaded = true;
        }
        finally
        {
            _loadSemaphore.Release();
        }
    }
}