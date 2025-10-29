using System.IO;

namespace Helpers
{
    public static class DataInitializer
    {
        private static readonly string SourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Data");
        private static readonly string TargetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MyApp");

        public static void EnsureProgramData()
        {
            try
            {
                if (!Directory.Exists(TargetPath))
                    Directory.CreateDirectory(TargetPath);

                foreach (string file in Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories))
                {
                    string relPath = Path.GetRelativePath(SourcePath, file);
                    string destFile = Path.Combine(TargetPath, relPath);

                    string destDir = Path.GetDirectoryName(destFile)!;
                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    if (!File.Exists(destFile))
                        File.Copy(file, destFile, overwrite: false);
                }
            }
            catch (Exception ex)
            {
                // Log or handle silently — permission issues?
                System.Diagnostics.Debug.WriteLine($"ProgramData init failed: {ex}");
            }
        }
    }
}