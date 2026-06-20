using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WormcraftLauncher
{
    public class ManifestFile
    {
        [JsonProperty("path")] public string Path { get; set; } = "";
        [JsonProperty("sha1")] public string Sha1 { get; set; } = "";
        [JsonProperty("size")] public long Size { get; set; }
    }

    public class Manifest
    {
        [JsonProperty("buildVersion")] public string BuildVersion { get; set; } = "";
        [JsonProperty("minecraft")] public string Minecraft { get; set; } = "1.19.2";
        [JsonProperty("forge")] public string Forge { get; set; } = "";
        [JsonProperty("serverIp")] public string ServerIp { get; set; } = "";
        [JsonProperty("baseUrl")] public string BaseUrl { get; set; } = "";
        [JsonProperty("files")] public List<ManifestFile> Files { get; set; } = new();
    }

    /// <summary>
    /// Локальные настройки лаунчера: ник, путь установки, текущая версия сборки.
    /// Хранится рядом с .exe в settings.json
    /// </summary>
    public class AppSettings
    {
        public string Nickname { get; set; } = "";
        public string InstallPath { get; set; } = "";
        public string InstalledBuildVersion { get; set; } = "";

        private static string SettingsPath =>
            System.IO.Path.Combine(AppContext.BaseDirectory, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var s = JsonConvert.DeserializeObject<AppSettings>(json);
                    if (s != null) return s;
                }
            }
            catch { /* игнорируем повреждённый файл настроек */ }

            return new AppSettings
            {
                InstallPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WormcraftLauncher", "game")
            };
        }

        public void Save()
        {
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
