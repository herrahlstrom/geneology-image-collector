using GenPhoto.Infrastructure;
using GenPhoto.ViewModels;
using System.Windows;

namespace GenPhoto.Views
{
    /// <summary>
    /// Interaction logic for NewMainWindow.xaml
    /// </summary>
    public partial class NewMainWindow : Window
    {
        public NewMainWindow()
        {
            DataContext = ServiceLocator.GetService<MainViewModel>();

            InitializeComponent();
        }
    }
}