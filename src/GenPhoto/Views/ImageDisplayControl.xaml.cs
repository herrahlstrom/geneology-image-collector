using GenPhoto.ViewModels;
using System.Windows.Controls;

namespace GenPhoto.Views;

/// <summary>
/// Interaction logic for ImageDisplayControl.xaml
/// </summary>
public partial class ImageDisplayControl : UserControl
{
    public ImageDisplayControl()
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