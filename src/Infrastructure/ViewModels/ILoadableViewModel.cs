using System.Windows.Input;

namespace GeneologyImageCollector.Infrastructure.ViewModels;

internal interface ILoadableViewModel
{
    public ICommand LoadCommand { get; }
}