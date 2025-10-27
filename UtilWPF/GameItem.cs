using System.Text.Json.Serialization;

namespace Utility
{
    
    public class GameItem
    {
        public string Name { get; set; }
        public string Subtitle { get; set; }
        public string Url { get; set; }
        public string IconPath { get; set; }
        public string ShortName { get; set; }
        public string AccentBrush { get; set; } = "#FF374151";
        public string Description { get; set; }

        // convenience
        [JsonIgnore]
        public bool HasIcon => !string.IsNullOrWhiteSpace(IconPath);
    }


}
