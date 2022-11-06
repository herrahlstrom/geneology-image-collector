using GenPhoto.Infrastructure.ViewModels;

namespace GenPhoto.ViewModels;

internal class PersonDisplayViewModel : ViewModelBase, IDisplayViewModel
{
    public Guid Id { get; init; }
    public string Name { get; set; } = "";
}