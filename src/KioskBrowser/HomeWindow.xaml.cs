using KioskBrowser.ViewModels;
using System.Windows;
using System.Windows.Input;

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
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowState = WindowState.Maximized;
            this.Topmost = false;

            vm = new GamesViewModel();
            DataContext = vm;
        }

        private bool _isFullScreen = false;
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;
        private ResizeMode _previousResizeMode;

        private void ToggleFullScreen()
        {
            if (!_isFullScreen)
            {
                _previousWindowState = this.WindowState;
                _previousWindowStyle = this.WindowStyle;
                _previousResizeMode = this.ResizeMode;

                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Maximized;
                this.Topmost = false;

                _isFullScreen = true;
            }
            else
            {
                this.WindowStyle = _previousWindowStyle;
                this.ResizeMode = _previousResizeMode;
                this.WindowState = _previousWindowState;
                this.Topmost = false;

                _isFullScreen = false;
            }
        }
        protected void OnKeyDown(object sender, KeyEventArgs e)
        {
            //base.OnKeyDown(e);
            if (e.Key == Key.F11)
            {
                ToggleFullScreen();
            }
        }

        
    }
}
