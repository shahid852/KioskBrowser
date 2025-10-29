using KioskBrowser.ViewModels;
using System.Diagnostics;
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
        private void RestartApp_Click(object sender, RoutedEventArgs e)
        {
            RestartApp();
        }

        public void RestartApp()
        {
            try
            {
                string exePath = Process.GetCurrentProcess().MainModule.FileName;

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(exePath)
                };

                Process.Start(psi);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to restart: " + ex.Message,
                    "Restart Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool _isFullScreen = true;
        //private WindowState _previousWindowState;
        //private WindowStyle _previousWindowStyle;
        //private ResizeMode _previousResizeMode;

        private void ToggleFullScreen()
        {
            if (!_isFullScreen)
            {
                //_previousWindowState = this.WindowState;
                //_previousWindowStyle = this.WindowStyle;
                //_previousResizeMode = this.ResizeMode;

                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Maximized;
                this.Topmost = false;

                _isFullScreen = true;
            }
            else
            {

                this.WindowStyle = WindowStyle.ThreeDBorderWindow;
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowState = WindowState.Maximized;
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
