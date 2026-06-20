using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.ProcessBuilder;

namespace WormcraftLauncher
{
    public class GameLauncher
    {
        private readonly string _gameDir;
        private readonly string _minecraftVersion;
        private readonly string _forgeVersion;
        private readonly string _serverIp;

        public GameLauncher(string gameDir, string minecraftVersion, string forgeVersion, string serverIp)
        {
            _gameDir = gameDir;
            _minecraftVersion = minecraftVersion;
            _forgeVersion = forgeVersion;
            _serverIp = serverIp;
        }

        public async Task<Process> LaunchAsync(string nickname, IProgress<string> log)
        {
            var path = new MinecraftPath(_gameDir);
            var launcher = new CMLauncher(path);

            launcher.FileChanged += (e) => log.Report($"[{e.FileKind}] {e.FileName} ({e.ProgressedFileCount}/{e.TotalFileCount})");
            launcher.ProgressChanged += (s, e) => log.Report($"Прогресс: {e.ProgressPercentage}%");

            // ID версии форджа в формате CmlLib, например "1.19.2-forge-43.4.0"
            var forgeOnlyVersion = _forgeVersion.Replace(_minecraftVersion + "-", "");
            var forgeVersionName = $"{_minecraftVersion}-forge-{forgeOnlyVersion}";

            log.Report("Проверка/установка Forge...");
            var forgeInstaller = new MForge(launcher);

            // Если версия ещё не установлена локально — ставим
            var allVersions = await launcher.GetAllVersionsAsync();
            bool alreadyInstalled = allVersions.Any(v => v.Name == forgeVersionName);
            if (!alreadyInstalled)
            {
                await forgeInstaller.Install(_minecraftVersion, forgeOnlyVersion);
            }

            var launchOption = new MLaunchOption
            {
                MaximumRamMb = 4096,
                MinimumRamMb = 2048,
                Session = MSession.CreateOfflineSession(nickname),
                ServerIp = _serverIp,
                ServerPort = 25565,
                FullScreen = false,
                ScreenWidth = 1280,
                ScreenHeight = 720,
            };

            log.Report("Запуск Minecraft...");
            var process = await launcher.CreateProcessAsync(forgeVersionName, launchOption);
            process.Start();
            return process;
        }
    }
}
