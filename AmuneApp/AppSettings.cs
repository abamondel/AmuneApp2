using Newtonsoft.Json;
using System.IO;

namespace AmuneApp
{
    public class AppSettings
    {
        private static readonly string SettingsPath = @"C:\ProgramData\AmuneApp\settings.json";

        public double WindowLeft { get; set; } = -1;
        public double WindowTop { get; set; } = -1;
        public double WindowOpacity { get; set; } = 1.0;
        public int DisplayTimerSeconds { get; set; } = 3;
        public bool ShuffleMode { get; set; } = false;
        public string FontFamily { get; set; } = "David";
        public bool DarkMode { get; set; } = false;
        public string BackgroundColor { get; set; } = "#CCFFFFFF";

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }

        public static AppSettings Load()
        {
            if (File.Exists(SettingsPath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
                catch { return new AppSettings(); }
            }
            return new AppSettings();
        }
    }
}
