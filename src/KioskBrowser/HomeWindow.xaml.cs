using KioskBrowser.ViewModels;
using System.Windows;

namespace KioskBrowser
{
    /// <summary>
    /// Interaction logic for HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {
        private readonly GamesViewModel vm;

        public HomeWindow()
        {
            InitializeComponent();
            vm = new GamesViewModel();
            DataContext = vm;
        }
    }
}
