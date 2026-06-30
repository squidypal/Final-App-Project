using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game2048.Core;
using Game2048.Persistence;

namespace Game2048.UI
{
    public sealed class SettingsPanel : Panel
    {
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle hapticsToggle;
        [SerializeField] private Toggle animationsToggle;
        [SerializeField] private Button themeButton;
        [SerializeField] private TextMeshProUGUI themeLabel;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI statsText;

        private GameManager game;

        private void Start()
        {
            game = GameManager.Instance;
            var settings = game.Settings;

            soundToggle.isOn = settings.SoundEnabled;
            hapticsToggle.isOn = settings.HapticsEnabled;
            animationsToggle.isOn = settings.AnimationsEnabled;

            soundToggle.onValueChanged.AddListener(value => settings.SoundEnabled = value);
            hapticsToggle.onValueChanged.AddListener(value => settings.HapticsEnabled = value);
            animationsToggle.onValueChanged.AddListener(value => settings.AnimationsEnabled = value);

            themeButton.onClick.AddListener(ToggleTheme);
            resetButton.onClick.AddListener(() =>
            {
                game.ResetProgress();
                RefreshStats();
            });
            closeButton.onClick.AddListener(() => SetVisible(false));

            UpdateThemeLabel();
        }

        public void Open()
        {
            RefreshStats();
            SetVisible(true);
        }

        private void ToggleTheme()
        {
            var settings = game.Settings;
            settings.Theme = settings.Theme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
            UpdateThemeLabel();
        }

        private void UpdateThemeLabel()
        {
            if (themeLabel != null)
            {
                themeLabel.text = game.Settings.Theme == ThemeMode.Light ? "Theme: Light" : "Theme: Dark";
            }
        }

        private void RefreshStats()
        {
            if (statsText == null)
            {
                return;
            }
            var stats = game.Stats;
            var builder = new StringBuilder();
            builder.AppendLine($"Games played:  {stats.GamesPlayed}");
            builder.AppendLine($"Games won:  {stats.GamesWon}");
            builder.AppendLine($"Best tile:  {stats.HighestTile}");
            builder.AppendLine($"Total moves:  {stats.TotalMoves}");
            builder.Append($"Total score:  {stats.TotalScore}");
            statsText.text = builder.ToString();
        }
    }
}
