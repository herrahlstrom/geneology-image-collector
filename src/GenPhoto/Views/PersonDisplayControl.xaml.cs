using GenPhoto.ViewModels;
using System.Windows.Controls;

namespace GenPhoto.Views;

/// <summary>
/// Interaction logic for PersonDisplayControl.xaml
/// </summary>
public partial class PersonDisplayControl : UserControl
{
    public PersonDisplayControl()
    {
        InitializeComponent();
        DataContextChanged += UserControl_DataContextChanged;
    }

    private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is IViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}