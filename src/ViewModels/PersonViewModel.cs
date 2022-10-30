namespace GeneologyImageCollector.ViewModels;

internal interface IDisplayViewModel
{ }

internal class PersonDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    public string Name { get; set; } = "";
}

internal class ImageDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    public string Title { get; set; } = "";
}