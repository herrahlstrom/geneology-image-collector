using GeneologyImageCollector.Infrastructure;

namespace GeneologyImageCollector.ViewModels;

internal class HistoryHolder<T> : ViewModelBase
{
    private Stack<T> m_back = new Stack<T>();
    private T? m_current;
    private Stack<T> m_forward = new Stack<T>();

    public HistoryHolder()
    {
        GoBackCommand = new RelayCommand(GoBack, () => m_back.Count > 0);
        GoForwardCommand = new RelayCommand(GoForward, () => m_forward.Count > 0);
    }

    public T? Current => m_current;
    public RelayCommand GoBackCommand { get; }
    public RelayCommand GoForwardCommand { get; }

    public void Add(T item)
    {
        if (m_current is not null)
        {
            m_back.Push(m_current);
            GoBackCommand.Revaluate();
        }

        m_current = item;
        OnPropertyChanged(nameof(Current));

        m_forward.Clear();
        GoForwardCommand.Revaluate();
    }

    private void GoBack()
    {
        if (m_back.TryPop(out T? item) && item != null)
        {
            GoBackCommand.Revaluate();
            if (m_current is not null)
            {
                m_forward.Push(m_current);
                GoForwardCommand.Revaluate();
            }

            m_current = item;
            OnPropertyChanged(nameof(Current));
        }
    }

    private void GoForward()
    {
        if (m_forward.TryPop(out T? item) && item != null)
        {
            GoForwardCommand.Revaluate();

            if (m_current is not null)
            {
                m_back.Push(m_current);
                GoBackCommand.Revaluate();
            }

            m_current = item;
            OnPropertyChanged(nameof(Current));
        }
    }
}