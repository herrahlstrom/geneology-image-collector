﻿namespace GenPhoto.ViewModels;

internal class PersonDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    public string Name { get; set; } = nameof(ImageDisplayViewModel);
}