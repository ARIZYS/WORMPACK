using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WormcraftLauncher
{
    public class UpdateProgress
    {
        public string Status { get; set; } = "";
        public int Percent { get; set; }
    }

    public class UpdateManager
    {
        // ВАЖНО: поменяй ТВОЙ_GITHUB_НИК и название репозитория на свои
        // Пример: https://cdn.jsdelivr.org/gh/ivan123/wormcraft-launcher@main/manifest.json
        private const string ManifestUrl =
            "https://cdn.jsdelivr.org/gh/ТВОЙ_GITHUB_НИК/wormcraft-launcher@main/manifest.json";

        private readonly HttpClient _http = new HttpClient();
        private readonly string _gameDir;

        public UpdateManager(string gameDir)
        {
            _gameDir = gameDir;
            Directory.CreateDirectory(_gameDir);
        }

        public async Task<Manifest> FetchManifestAsync()
        {
            var json = await _http.GetStringAsync(ManifestUrl);
            var manifest = JsonConvert.DeserializeObject<Manifest>(json);
            if (manifest == null) throw new Exception("Не удалось разобрать manifest.json");
            return manifest;
        }

        /// <summary>
        /// Сравнивает локальные файлы сборки с манифестом и приводит их в соответствие:
        /// докачивает новые/изменённые, удаляет те, которых больше нет в манифесте.
        /// </summary>
        public async Task SyncAsync(Manifest manifest, IProgress<UpdateProgress> progress)
        {
            var modpackRoot = Path.Combine(_gameDir, "modpack");
            Directory.CreateDirectory(modpackRoot);

            var remoteFiles = manifest.Files;
            var toDownload = new List<ManifestFile>();

            foreach (var f in remoteFiles)
            {
                var localPath = Path.Combine(modpackRoot, f.Path.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(localPath))
                {
                    toDownload.Add(f);
                    continue;
                }

                var localHash = await Sha1OfFileAsync(localPath);
                if (!string.Equals(localHash, f.Sha1, StringComparison.OrdinalIgnoreCase))
                {
                    toDownload.Add(f);
                }
            }

            // Удаляем локальные файлы, которых больше нет в манифесте (старые моды/конфиги)
            var remoteRelPaths = remoteFiles
                .Select(f => f.Path.Replace('/', Path.DirectorySeparatorChar))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (Directory.Exists(modpackRoot))
            {
                foreach (var existing in Directory.GetFiles(modpackRoot, "*", SearchOption.AllDirectories))
                {
                    var rel = Path.GetRelativePath(modpackRoot, existing);
                    if (!remoteRelPaths.Contains(rel))
                    {
                        try { File.Delete(existing); } catch { /* не критично */ }
                    }
                }
            }

            // Скачиваем новые/изменённые файлы
            int done = 0;
            int total = Math.Max(toDownload.Count, 1);

            foreach (var f in toDownload)
            {
                progress.Report(new UpdateProgress
                {
                    Status = $"Скачивание: {f.Path}",
                    Percent = (int)((done / (double)total) * 100)
                });

                var localPath = Path.Combine(modpackRoot, f.Path.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

                var url = manifest.BaseUrl.TrimEnd('/') + "/" + f.Path;
                var bytes = await _http.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(localPath, bytes);

                done++;
            }

            progress.Report(new UpdateProgress { Status = "Сборка обновлена", Percent = 100 });

            // Копируем ВСЕ папки сборки (mods, config, shaderpacks, scripts, emotes
            // и любые другие, какие есть в манифесте) из modpack в реальную папку .minecraft
            CopyModpackIntoMinecraft(modpackRoot, _gameDir, remoteFiles);
        }

        /// <summary>
        /// Зеркалит ЛЮБЫЕ папки верхнего уровня, перечисленные в манифесте
        /// (mods, config, shaderpacks, scripts, emotes, resourcepacks и т.д.)
        /// из скачанной сборки в реальную папку .minecraft.
        /// </summary>
        private void CopyModpackIntoMinecraft(string modpackRoot, string minecraftDir, List<ManifestFile> remoteFiles)
        {
            // Собираем уникальный список папок верхнего уровня из путей манифеста.
            // Например из "mods/jei.jar" и "shaderpacks/BSL.zip" получим { "mods", "shaderpacks" }.
            var topLevelFolders = remoteFiles
                .Select(f => f.Path.Split('/')[0])
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var folder in topLevelFolders)
            {
                var src = Path.Combine(modpackRoot, folder);
                if (!Directory.Exists(src)) continue;

                var dst = Path.Combine(minecraftDir, folder);
                Directory.CreateDirectory(dst);

                // Зеркалим: удаляем в .minecraft то, чего нет в скачанной сборке
                var srcRel = Directory.GetFiles(src, "*", SearchOption.AllDirectories)
                    .Select(p => Path.GetRelativePath(src, p)).ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (Directory.Exists(dst))
                {
                    foreach (var existing in Directory.GetFiles(dst, "*", SearchOption.AllDirectories))
                    {
                        var rel = Path.GetRelativePath(dst, existing);
                        if (!srcRel.Contains(rel))
                        {
                            try { File.Delete(existing); } catch { }
                        }
                    }
                }

                foreach (var file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
                {
                    var rel = Path.GetRelativePath(src, file);
                    var target = Path.Combine(dst, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    File.Copy(file, target, overwrite: true);
                }
            }
        }

        private static async Task<string> Sha1OfFileAsync(string path)
        {
            using var sha1 = SHA1.Create();
            using var stream = File.OpenRead(path);
            var hash = await Task.Run(() => sha1.ComputeHash(stream));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
