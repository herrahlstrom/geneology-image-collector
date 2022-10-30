using GeneologyImageCollector.Infrastructure;
using GeneologyImageCollector.Infrastructure.ViewModels;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows.Input;

namespace GeneologyImageCollector.ViewModels;

internal interface IMainViewModel : ILoadableViewModel
{ }

internal class MainViewModel : IMainViewModel
{
    private readonly IConfiguration m_config;

    public MainViewModel(IConfiguration config)
    {
        LoadCommand = new RelayCommand(LoadCommand_Execute);
        m_config = config;
    }

    public ICommand LoadCommand { get; }

    private void LoadCommand_Execute()
    {
    }
}