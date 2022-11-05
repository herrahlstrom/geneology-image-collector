using GenPhoto.Infrastructure;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace GenPhoto.ViewModels;

internal interface IViewModel
{
    public IRelayCommand LoadCommand { get; }
}

internal abstract class ViewModelBase : IViewModel, INotifyPropertyChanged
{
    private string _errorMessage = "";

    public ViewModelBase()
    {
        LoadCommand = new RelayCommand(LoadCommand_Execute);

        OpenFileCommand = new RelayCommand<string>(
            canExecute: path => File.Exists(path),
            execute: path => Process.Start(new ProcessStartInfo("explorer", path!)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ErrorMessage
    {
        get { return _errorMessage; }
        set { SetProperty(ref _errorMessage, value); }
    }

    public IRelayCommand LoadCommand { get; }
    public IRelayCommand OpenFileCommand { get; }

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