using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game2048.Core;

namespace Game2048.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreValue;
        [SerializeField] private TextMeshProUGUI bestValue;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button settingsButton;

        private GameManager game;

        private void Start()
        {
            game = GameManager.Instance;
            if (game == null)
            {
                return;
            }

            game.Score.CurrentChanged += OnScoreChanged;
            game.Score.BestChanged += OnBestChanged;
            OnScoreChanged(game.Score.Current);
            OnBestChanged(game.Score.Best);

            newGameButton.onClick.AddListener(() => game.Game.NewGame());
            undoButton.onClick.AddListener(() => game.Game.Undo());
            settingsButton.onClick.AddListener(() => game.SettingsPanel.Open());
        }

        private void OnDestroy()
        {
            if (game != null)
            {
                game.Score.CurrentChanged -= OnScoreChanged;
                game.Score.BestChanged -= OnBestChanged;
            }
        }

        private void OnScoreChanged(int value)
        {
            scoreValue.text = value.ToString();
        }

        private void OnBestChanged(int value)
        {
            bestValue.text = value.ToString();
        }
    }
}
