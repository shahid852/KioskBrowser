using Utility;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Threading; // dispatcher
using Timer = System.Timers.Timer;
namespace KioskBrowser.ViewModels
{


    public class GamesProvider : IDisposable
    {
        private readonly string _path;
        private readonly FileSystemWatcher _watcher;
        private readonly System.Timers.Timer _debounceTimer;
        private readonly Dispatcher _uiDispatcher;
        public event Action<List<GameItem>> GamesChanged;

        public GamesProvider(Dispatcher uiDispatcher)
        {
            _uiDispatcher = uiDispatcher;
            _path = Helpers.GetGamesFilePath();

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_path));

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_path) ?? ".", Path.GetFileName(_path))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
            };

            _watcher.Changed += OnFsChanged;
            _watcher.Created += OnFsChanged;
            _watcher.Renamed += OnFsChanged;
            _watcher.EnableRaisingEvents = true;

            _debounceTimer = new Timer(250); // wait 250ms to avoid multiple events
            _debounceTimer.AutoReset = false;
            _debounceTimer.Elapsed += (s, e) => ReloadAndRaise();
        }

        public List<GameItem> LoadOnce()
        {
            return TryLoadFile();
        }

        private void OnFsChanged(object sender, FileSystemEventArgs e)
        {
            // start/refresh debounce timer
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void ReloadAndRaise()
        {
            var list = TryLoadFile();
            if (list != null)
            {
                // marshal to UI thread when raising so subscribers can update ObservableCollection safely
                _uiDispatcher.BeginInvoke(new Action(() => GamesChanged?.Invoke(list)));
            }
        }

        private List<GameItem> TryLoadFile()
        {
            try
            {
                if (!File.Exists(_path)) return new List<GameItem>();

                // small delay if file is locked - try a few times
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        using var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        var list = JsonSerializer.Deserialize<List<GameItem>>(fs) ?? new List<GameItem>();
                        return list;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load games failed: {ex}");
            }
            return new List<GameItem>();
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _debounceTimer?.Dispose();
        }
    }

}
