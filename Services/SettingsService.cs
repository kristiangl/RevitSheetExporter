using Newtonsoft.Json;
using RevitSheetExporter.Models;
using System.IO;

namespace RevitSheetExporter.Services
{
    /// <summary>
    /// Saves and loads ExportSettings as JSON.
    /// Default path: %AppData%\RevitSheetExporter\settings.json
    /// To transfer settings between computers, just copy that file.
    /// </summary>
    public class SettingsService
    {
        public static readonly string DefaultSettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RevitSheetExporter",
            "settings.json"
        );

        public ExportSettings Load(string? path = null)
        {
            var filePath = path ?? DefaultSettingsPath;

            if (!File.Exists(filePath))
                return new ExportSettings();

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<ExportSettings>(json) ?? new ExportSettings();
            }
            catch
            {
                return new ExportSettings();
            }
        }

        public void Save(ExportSettings settings, string? path = null)
        {
            var filePath = path ?? DefaultSettingsPath;
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
    }
}
