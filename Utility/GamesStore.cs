using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Utility
{
    public static class GamesStore
    {
        private static readonly string GamesFilePath = Helpers.GetGamesFilePath();

        public static List<GameItem> LoadGames()
        {
            try
            {
                if (!File.Exists(GamesFilePath))
                    return new List<GameItem>();

                var json = File.ReadAllText(GamesFilePath);
                var items = JsonSerializer.Deserialize<List<GameItem>>(json);
                return items ?? new List<GameItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GamesStore.LoadGames() failed: {ex.Message}");
                return new List<GameItem>();
            }
        }

        public static async Task SaveGamesAsync(IEnumerable<GameItem> items)
        {
            var path = GamesFilePath;
            var tmp = path + ".tmp";

            var options = new JsonSerializerOptions { WriteIndented = true };
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(fs, items, options).ConfigureAwait(false);
                await fs.FlushAsync().ConfigureAwait(false);
            }

            // atomic replace
            File.Copy(tmp, path, overwrite: true);
            File.Delete(tmp);
        }
    }
}
