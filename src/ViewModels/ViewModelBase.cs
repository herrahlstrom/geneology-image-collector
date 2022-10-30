using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GeneologyImageCollector.ViewModels;

internal abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

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