using System.Windows.Input;

namespace GenPhoto.Infrastructure.ViewModels;

internal interface ILoadableViewModel
{
    public IRelayCommand LoadCommand { get; }
}