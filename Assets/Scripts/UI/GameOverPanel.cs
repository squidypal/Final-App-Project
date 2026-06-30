using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game2048.Core;

namespace Game2048.UI
{
    public sealed class GameOverPanel : Panel
    {
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button tryAgainButton;

        private void Start()
        {
            tryAgainButton.onClick.AddListener(() =>
            {
                SetVisible(false);
                GameManager.Instance.Game.NewGame();
            });
        }

        public void Show(int score)
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = score.ToString();
            }
            SetVisible(true);
        }
    }
}
