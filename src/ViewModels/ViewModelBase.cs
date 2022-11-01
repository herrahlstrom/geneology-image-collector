using GeneologyImageCollector.Infrastructure;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace GeneologyImageCollector.ViewModels;

internal interface IViewModel
{
    public ICommand LoadCommand { get; }
}

internal abstract class ViewModelBase : IViewModel, INotifyPropertyChanged
{
    private string _errorMessage = "";

    public ViewModelBase()
    {
        LoadCommand = new RelayCommand(LoadCommand_Execute);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ErrorMessage
    {
        get { return _errorMessage; }
        set { SetProperty(ref _errorMessage, value); }
    }

    public ICommand LoadCommand { get; }

    protected virtual void LoadCommand_Execute()
    {
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
    {
        if (field is null || !field.Equals(value))
        {
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}