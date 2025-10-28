using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Utility;

namespace KioskBrowser.ViewModels
{
    public class GamesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<GameItem> Games { get; } = new ObservableCollection<GameItem>();

        // Exposed filtered view (simple example, you can use CollectionView for advanced filtering)
        public ObservableCollection<GameItem> FilteredGames { get; } = new ObservableCollection<GameItem>();

        public ICommand LaunchGameCommand { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                RebuildFiltered();
            }
        }

        public GamesViewModel()
        {
            // sample command; wire to Process.Start or app-specific launcher
            LaunchGameCommand = new RelayCommand<string>(url => Launcher.Launch(url));

            // sample initial population (in production load from JSON/DB)
            //Games.Add(new GameItem { Name = "Game 1", Url = "https://www.pragmaticplay.com/en/games/777-rush/?gamelang=en&cur=USD#", Subtitle = "Game 1", ShortName = "MS", AccentBrush = "#FF6EE7B7" });
            //Games.Add(new GameItem { Name = "Game 2", Url = "https://static-stage.contentmedia.eu/ecf3/index.html?gameid=10234&operatorid=44&currency=EU[…]u%2Fcapi&papi=https%3A%2F%2Fpapi-stage.contentmedia.eu", Subtitle = "Game 2", ShortName = "R", AccentBrush = "#FF93C5FD" });
            //Games.Add(new GameItem { Name = "Game 3", Url = "https://games.netent.com/video-slots/gonzos-quest/", Subtitle = "Game 3", ShortName = "BJ", AccentBrush = "#FFFCA5A5" });
            // ... add more
           
            var list = GamesStore.LoadGames();

            Games = new ObservableCollection<GameItem>(list);

            RebuildFiltered();
        }

        private void RebuildFiltered()
        {
            FilteredGames.Clear();
            var subset = string.IsNullOrWhiteSpace(SearchText)
                ? Games
                : new ObservableCollection<GameItem>(Games.Where(g => g.Name.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase)));

            foreach (var g in subset) FilteredGames.Add(g);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // very small RelayCommand
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        { _execute = execute ?? throw new ArgumentNullException(nameof(execute)); _canExecute = canExecute; }
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
        public void Execute(object parameter) => _execute((T)parameter);
        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // Sample launcher; in production adapt to your kioskbrowser logic and single-instance behavior
    public static class Launcher
    {
        public static void Launch(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            try
            {
                // For kioskbrowser.exe with url as single quoted parameter:
                //var exe = "kioskbrowser.exe";
                //System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                //{
                //    FileName = exe,
                //    Arguments = $"\"{url}\"",
                //    UseShellExecute = true
                //});

                var window = new MainWindow();
                window.Show();
            }
            catch (Exception ex)
            {
                // TODO: log or show error
                System.Diagnostics.Debug.WriteLine($"Launch failed: {ex.Message}");
            }
        }
    }

}
