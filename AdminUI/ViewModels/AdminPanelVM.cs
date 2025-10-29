using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Utility;
namespace AdminUI.ViewModels
{

    public class AdminPanelViewModel : ObservableObject
    {
        // Change these to match where kioskbrowser is installed
        private const string KioskExeName = "kioskbrowser"; // process name without .exe
        private string KioskExePath = Utility.Helpers.GetKioskExe(); 
            //@"C:\Users\P\source\repos\KioskBrowser\src\KioskBrowser\bin\x64\Debug\kioskbrowser.exe"; //@"C:\Program Files\KioskBrowser\kioskbrowser.exe";

        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "KioskBrowser",
            "adminpanel.log");

        public ObservableCollection<GameItem> Games { get; } = new ObservableCollection<GameItem>();

        private GameItem _selected;
        public GameItem SelectedGame
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public ICommand AddGameCommand { get; }
        public ICommand RemoveGameCommand { get; }
        public IAsyncCommand SaveAndRestartCommand { get; }

        public AdminPanelViewModel()
        {
            // load initial games from store
            try
            {
                var loaded = GamesStore.LoadGames();
                foreach (var g in loaded) Games.Add(g);
            }
            catch (Exception ex)
            {
                Log($"ERROR: Failed to load games at startup: {ex}");
                MessageBox.Show($"Failed to load existing games: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            AddGameCommand = new RelayCommand(AddGame);
            RemoveGameCommand = new RelayCommand(RemoveSelected, () => SelectedGame != null);
            SaveAndRestartCommand = new AsyncRelayCommand(SaveAndRestartAsync, CanSaveAndRestart);

            // watch selection change to update Remove command availability
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedGame))
                {
                    (RemoveGameCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            };
        }

        private void AddGame()
        {
            var g = new GameItem
            {
                Name = "New Game",
                Subtitle = "",
                Url = "https://",
                ShortName = "NG",
                AccentBrush = "#FF374151",
                IconPath = ""
            };
            Games.Add(g);
            SelectedGame = g;
        }

        private void RemoveSelected()
        {
            if (SelectedGame != null)
            {
                var toRemove = SelectedGame;
                Games.Remove(toRemove);
                SelectedGame = Games.FirstOrDefault();
            }
        }

        private bool CanSaveAndRestart() => Games.Count >= 0;

        private async Task SaveAndRestartAsync()
        {
            try
            {
                Log("Save & Restart requested.");

                // Ask user to confirm
                var res = MessageBox.Show("Save changes and restart KioskBrowser now?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes)
                {
                    Log("User cancelled Save & Restart.");
                    return;
                }

                // Save (atomic inside GamesStore)
                Log($"Saving {Games.Count} games to disk...");
                await GamesStore.SaveGamesAsync(Games.ToList()).ConfigureAwait(false);
                Log("Save completed.");

                // Restart kiosk (force restart: kill then start)
                Log("Attempting kiosk restart...");
                var ok = await RestartKioskAsync().ConfigureAwait(false);

                // Show result (marshal to UI)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ok)
                    {
                        MessageBox.Show("Saved changes and restarted KioskBrowser successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Log("Kiosk restarted successfully.");
                    }
                    else
                    {
                        MessageBox.Show("Saved changes, but failed to restart KioskBrowser. See log for details.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Log("WARNING: Kiosk restart reported failure.");
                    }
                });
            }
            catch (Exception ex)
            {
                Log($"EXCEPTION in SaveAndRestartAsync: {ex}");
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show($"An error occurred while saving or restarting the kiosk: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                );
            }
        }

        private Task<bool> RestartKioskAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Kill existing processes by name
                    var running = Process.GetProcessesByName(KioskExeName);
                    if (running.Length > 0)
                    {
                        foreach (var p in running)
                        {
                            try
                            {
                                Log($"Killing process Id={p.Id}");
                                p.Kill(); // use Kill(true) on .NET 5+ if you want tree kill
                                p.WaitForExit(3000);
                                Log($"Killed {p.Id}");
                            }
                            catch (Exception exKill)
                            {
                                Log($"Failed to kill {p.Id}: {exKill}");
                            }
                        }
                    }
                    else
                    {
                        Log("No running kioskbrowser processes found.");
                    }

                    // Start new process
                    var psi = new ProcessStartInfo
                    {
                        FileName = KioskExePath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(KioskExePath) ?? Environment.CurrentDirectory
                    };

                    Process.Start(psi);
                    Log($"Started new process using '{psi.FileName}'.");

                    // Quick verification
                    var sw = Stopwatch.StartNew();
                    while (sw.ElapsedMilliseconds < 3000)
                    {
                        var exists = Process.GetProcessesByName(KioskExeName).Any();
                        if (exists) return true;
                        Task.Delay(200).Wait();
                    }

                    Log("Warning: New kioskbrowser process not detected within timeout.");
                    return false;
                }
                catch (Exception ex)
                {
                    Log($"RestartKioskAsync failed: {ex}");
                    return false;
                }
            });
        }

        private static void Log(string message)
        {
            try
            {
                var dir = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var line = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ssZ} {message}";
                File.AppendAllLines(LogPath, new[] { line });
            }
            catch
            {
                // swallow logging errors
            }
        }
    }
}


