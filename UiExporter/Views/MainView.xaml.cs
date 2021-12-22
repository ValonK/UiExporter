using System.Windows;
using UiExporter.ViewModels;

namespace UiExporter.Views
{
    public partial class MainView : Window
    {
        private readonly MainViewModel _mainViewModel;

        public MainView()
        {
            InitializeComponent();

            Height = 100;
            _mainViewModel = new MainViewModel();
            DataContext = _mainViewModel;
        }
    }
}
