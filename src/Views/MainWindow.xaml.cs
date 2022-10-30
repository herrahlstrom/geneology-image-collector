using GeneologyImageCollector.ViewModels;
using System.Windows;

namespace GeneologyImageCollector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = ServiceLocator.GetService<IMainViewModel>();

            InitializeComponent();
        }
    }
}