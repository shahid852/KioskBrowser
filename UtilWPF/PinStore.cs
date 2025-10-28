using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Utility;

namespace AdminUI.Security
{
    public static class PinStore
    {
        private static readonly string PinFilePath =
            Path.Combine(Helpers.GetConfigFolder(), "pin.json");

        private const string DefaultPin = "1234";

        private class PinData
        {
            public string PinHash { get; set; } = string.Empty;
        }

        /// <summary>
        /// Ensures that the PIN file exists. Creates it with default PIN if missing.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                if (!File.Exists(PinFilePath))
                {
                    var defaultHash = ComputeHash(DefaultPin);
                    var data = new PinData { PinHash = defaultHash };

                    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(PinFilePath, json);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to initialize PIN storage:\n{ex.Message}",
                    "Initialization Error", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Verifies the entered PIN by comparing its hash to the stored one.
        /// </summary>
        public static async Task<bool> VerifyPinAsync(string enteredPin)
        {
            try
            {
                if (!File.Exists(PinFilePath))
                {
                    Initialize();
                }

                using var fs = new FileStream(PinFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var data = await JsonSerializer.DeserializeAsync<PinData>(fs).ConfigureAwait(false);
                if (data == null || string.IsNullOrEmpty(data.PinHash))
                    return false;

                string enteredHash = ComputeHash(enteredPin);
                return string.Equals(enteredHash, data.PinHash, StringComparison.InvariantCultureIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
        public static bool VerifyPin(string enteredPin)
        {
            try
            {
                if (!File.Exists(PinFilePath))
                {
                    Initialize();
                }

                using var fs = new FileStream(PinFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var data = JsonSerializer.Deserialize<PinData>(fs);
                if (data == null || string.IsNullOrEmpty(data.PinHash))
                    return false;

                string enteredHash = ComputeHash(enteredPin);
                return string.Equals(enteredHash, data.PinHash, StringComparison.InvariantCultureIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Changes the PIN to a new one and securely updates the file.
        /// </summary>
        public static async Task ChangePinAsync(string newPin)
        {
            try
            {
                var newHash = ComputeHash(newPin);
                var data = new PinData { PinHash = newHash };
                var tmpPath = PinFilePath + ".tmp";

                var options = new JsonSerializerOptions { WriteIndented = true };

                using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await JsonSerializer.SerializeAsync(fs, data, options).ConfigureAwait(false);
                    await fs.FlushAsync().ConfigureAwait(false);
                }

                File.Copy(tmpPath, PinFilePath, overwrite: true);
                File.Delete(tmpPath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to change PIN:\n{ex.Message}",
                    "Error", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
        public static async Task<bool> ChangePinAsync(string oldPin, string newPin)
        {
            try
            {
                if (!await VerifyPinAsync(oldPin).ConfigureAwait(false))
                    return false;

                var newHash = ComputeHash(newPin);
                var data = new PinData { PinHash = newHash };
                var options = new JsonSerializerOptions { WriteIndented = true };

                using var fs = new FileStream(PinFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(fs, data, options).ConfigureAwait(false);
                await fs.FlushAsync().ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
