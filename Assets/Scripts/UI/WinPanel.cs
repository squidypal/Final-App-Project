using UnityEngine;
using UnityEngine.UI;
using Game2048.Core;

namespace Game2048.UI
{
    public sealed class WinPanel : Panel
    {
        [SerializeField] private Button keepGoingButton;
        [SerializeField] private Button newGameButton;

        private void Start()
        {
            keepGoingButton.onClick.AddListener(() =>
            {
                SetVisible(false);
                GameManager.Instance.Game.ContinueAfterWin();
            });

            newGameButton.onClick.AddListener(() =>
            {
                SetVisible(false);
                GameManager.Instance.Game.NewGame();
            });
        }

        public void Show()
        {
            SetVisible(true);
        }
    }
}
