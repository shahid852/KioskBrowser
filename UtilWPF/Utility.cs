using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Utility
{
    public static class Helpers
    {
        public static string ProgramData = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Data"); //Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "KioskBrowser");
        public static string GetGamesFilePath()
        {
            var dir = ProgramData;
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "games.json"); // C:\ProgramData\KioskBrowser\games.json
        }
        public static string GetConfigFolder()
        {
            var dir = ProgramData; //Path.Combine(UtilWPF.Properties.Settings.Default.SettingsPath);
            Directory.CreateDirectory(dir);
            return Path.Combine(dir);
        }
        //public static string GetKioskExe()
        //{
        //    var dir = Path.Combine(UtilWPF.Properties.Settings.Default.KioskBrowserPath);
        //    Directory.CreateDirectory(dir);
        //    return Path.Combine(dir, "kioskbrowser.exe"); // C:\ProgramData\KioskBrowser\games.json
        //}

        public static void ForceRestartKiosk()
        {
            foreach (var p in Process.GetProcessesByName("kioskbrowser"))
            {
                try { p.Kill(); }
                catch { /* log */ }
            }

            Process.Start(@"C:\Program Files\KioskBrowser\kioskbrowser.exe");
        }


    }
}
