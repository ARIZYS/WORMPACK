using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace WormcraftLauncher
{
    public class MainForm : Form
    {
        private readonly AppSettings _settings;

        private TextBox _nicknameBox = new();
        private Button _playButton = new();
        private ProgressBar _progressBar = new();
        private Label _statusLabel = new();
        private TextBox _logBox = new();

        public MainForm()
        {
            _settings = AppSettings.Load();

            Text = "Wormcraft Launcher";
            Width = 560;
            Height = 460;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(24, 24, 28);

            BuildUi();
        }

        private void BuildUi()
        {
            var title = new Label
            {
                Text = "WORMCRAFT  •  wormcraft.20tps.ru",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(title);

            var nickLabel = new Label
            {
                Text = "Никнейм:",
                ForeColor = Color.Gainsboro,
                AutoSize = true,
                Location = new Point(20, 70)
            };
            Controls.Add(nickLabel);

            _nicknameBox.Location = new Point(20, 95);
            _nicknameBox.Width = 300;
            _nicknameBox.Font = new Font("Segoe UI", 11);
            _nicknameBox.Text = _settings.Nickname;
            _nicknameBox.MaxLength = 16;
            Controls.Add(_nicknameBox);

            _playButton.Text = "ИГРАТЬ";
            _playButton.Location = new Point(340, 93);
            _playButton.Size = new Size(180, 36);
            _playButton.BackColor = Color.FromArgb(70, 170, 90);
            _playButton.ForeColor = Color.White;
            _playButton.FlatStyle = FlatStyle.Flat;
            _playButton.FlatAppearance.BorderSize = 0;
            _playButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _playButton.Click += async (s, e) => await OnPlayClicked();
            Controls.Add(_playButton);

            _statusLabel.Text = "Готов к запуску";
            _statusLabel.ForeColor = Color.Gainsboro;
            _statusLabel.AutoSize = true;
            _statusLabel.Location = new Point(20, 145);
            Controls.Add(_statusLabel);

            _progressBar.Location = new Point(20, 170);
            _progressBar.Width = 500;
            _progressBar.Height = 18;
            Controls.Add(_progressBar);

            _logBox.Location = new Point(20, 200);
            _logBox.Size = new Size(500, 220);
            _logBox.Multiline = true;
            _logBox.ReadOnly = true;
            _logBox.ScrollBars = ScrollBars.Vertical;
            _logBox.BackColor = Color.FromArgb(18, 18, 20);
            _logBox.ForeColor = Color.LightGray;
            _logBox.BorderStyle = BorderStyle.FixedSingle;
            _logBox.Font = new Font("Consolas", 9);
            Controls.Add(_logBox);
        }

        private void Log(string line)
        {
            if (_logBox.InvokeRequired)
            {
                _logBox.Invoke(() => AppendLog(line));
            }
            else
            {
                AppendLog(line);
            }
        }

        private void AppendLog(string line)
        {
            _logBox.AppendText(line + Environment.NewLine);
        }

        private async Task OnPlayClicked()
        {
            var nickname = _nicknameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(nickname) || nickname.Length < 3)
            {
                MessageBox.Show("Введи никнейм от 3 до 16 символов.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _settings.Nickname = nickname;
            _settings.Save();

            _playButton.Enabled = false;
            _logBox.Clear();

            try
            {
                _statusLabel.Text = "Проверка обновлений...";
                var updateManager = new UpdateManager(_settings.InstallPath);
                var manifest = await updateManager.FetchManifestAsync();

                Log($"Версия сборки на сервере: {manifest.BuildVersion}");
                Log($"Установленная версия: {_settings.InstalledBuildVersion}");

                if (manifest.BuildVersion != _settings.InstalledBuildVersion)
                {
                    _statusLabel.Text = "Обновление сборки модов...";
                    var progress = new Progress<UpdateProgress>(p =>
                    {
                        _progressBar.Value = Math.Min(p.Percent, 100);
                        _statusLabel.Text = p.Status;
                    });

                    await updateManager.SyncAsync(manifest, progress);

                    _settings.InstalledBuildVersion = manifest.BuildVersion;
                    _settings.Save();
                    Log("Сборка обновлена до версии " + manifest.BuildVersion);
                }
                else
                {
                    Log("Сборка уже актуальна, обновление не требуется.");
                }

                _statusLabel.Text = "Запуск игры...";
                var gameLauncher = new GameLauncher(
                    _settings.InstallPath,
                    manifest.Minecraft,
                    manifest.Forge,
                    manifest.ServerIp);

                var logProgress = new Progress<string>(Log);
                var process = await gameLauncher.LaunchAsync(nickname, logProgress);

                _statusLabel.Text = "Игра запущена";
                Log("Minecraft запущен. Окно лаунчера можно свернуть.");

                // Сворачиваем лаунчер пока игра открыта (опционально)
                process.Exited += (s, e) =>
                {
                    Invoke(() =>
                    {
                        _statusLabel.Text = "Игра закрыта";
                        _playButton.Enabled = true;
                    });
                };
                process.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Log("ОШИБКА: " + ex.Message);
                _statusLabel.Text = "Ошибка запуска";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _playButton.Enabled = true;
            }
        }
    }
}
