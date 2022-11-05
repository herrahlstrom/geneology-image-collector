using System.Windows.Input;

namespace GenPhoto.Infrastructure;

public interface IRelayCommand : ICommand
{
    void Revaluate();
}

public class RelayCommand<T> : IRelayCommand
{
    private readonly Predicate<T?> _canExecute;
    private readonly Action<T?> _execute;
    private int _executionCounter;

    public RelayCommand(Action<T?> execute, Predicate<T?> canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public RelayCommand(Action<T?> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = AlwaysTrue;
    }

    public event EventHandler? CanExecuteChanged;

    public bool SingleExecution { get; init; }

    public bool CanExecute(object? parameter)
    {
        if (SingleExecution && _executionCounter > 0)
        {
            return false;
        }

        return parameter is T p
            ? _canExecute(p)
            : _canExecute(default);
    }

    public void Execute(object? parameter)
    {
        if (parameter is T obj)
        {
            _execute(obj);
        }
        else
        {
            _execute(default);
        }

        _executionCounter++;
        if (SingleExecution)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Revaluate() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    private static bool AlwaysTrue(T? _) => true;
}

public class RelayCommand : RelayCommand<object>
{
    public RelayCommand(Action execute, Func<bool> canExecute)
        : base(_ => execute.Invoke(), _ => canExecute.Invoke()) { }

    public RelayCommand(Action execute) : base(_ => execute.Invoke())
    {
    }
}