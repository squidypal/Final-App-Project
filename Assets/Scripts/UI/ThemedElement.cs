using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game2048.Core;
using Game2048.Services;

namespace Game2048.UI
{
    public enum ThemeRole
    {
        Background,
        Panel,
        PrimaryText,
        SecondaryText,
        AccentFill,
        AccentText
    }

    public sealed class ThemedElement : MonoBehaviour
    {
        [SerializeField] private ThemeRole role;

        private ThemeService theme;
        private Graphic graphic;

        private void Start()
        {
            graphic = GetComponent<Graphic>();
            if (GameManager.Instance != null)
            {
                theme = GameManager.Instance.Theme;
                theme.Changed += Apply;
            }
            Apply();
        }

        private void OnDestroy()
        {
            if (theme != null)
            {
                theme.Changed -= Apply;
            }
        }

        private void Apply()
        {
            if (graphic == null || theme == null)
            {
                return;
            }

            var palette = theme.Current;
            switch (role)
            {
                case ThemeRole.Background:
                    graphic.color = palette.Background;
                    break;
                case ThemeRole.Panel:
                    graphic.color = palette.Panel;
                    break;
                case ThemeRole.PrimaryText:
                    graphic.color = palette.PrimaryText;
                    break;
                case ThemeRole.SecondaryText:
                    graphic.color = palette.SecondaryText;
                    break;
                case ThemeRole.AccentFill:
                    graphic.color = palette.Accent;
                    break;
                case ThemeRole.AccentText:
                    graphic.color = palette.AccentText;
                    break;
            }
        }
    }
}
