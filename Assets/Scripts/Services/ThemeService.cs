using System;
using UnityEngine;
using Game2048.Persistence;

namespace Game2048.Services
{
    public struct ThemePalette
    {
        public Color Background;
        public Color BoardBackground;
        public Color EmptyCell;
        public Color PrimaryText;
        public Color SecondaryText;
        public Color Panel;
        public Color Accent;
        public Color AccentText;
    }

    public sealed class ThemeService
    {
        private readonly SettingsService settings;

        public event Action Changed;

        public ThemeService(SettingsService settings)
        {
            this.settings = settings;
            this.settings.Changed += () => Changed?.Invoke();
        }

        public ThemeMode Mode => settings.Theme;

        public ThemePalette Current => Mode == ThemeMode.Dark ? Dark() : Light();

        private static ThemePalette Light()
        {
            return new ThemePalette
            {
                Background = TilePalette.Hex("#FAF8EF"),
                BoardBackground = TilePalette.Hex("#BBADA0"),
                EmptyCell = TilePalette.Hex("#CDC1B4"),
                PrimaryText = TilePalette.Hex("#776E65"),
                SecondaryText = TilePalette.Hex("#F9F6F2"),
                Panel = TilePalette.Hex("#FAF8EF"),
                Accent = TilePalette.Hex("#8F7A66"),
                AccentText = TilePalette.Hex("#F9F6F2")
            };
        }

        private static ThemePalette Dark()
        {
            return new ThemePalette
            {
                Background = TilePalette.Hex("#1B1A17"),
                BoardBackground = TilePalette.Hex("#3A352F"),
                EmptyCell = TilePalette.Hex("#4A443C"),
                PrimaryText = TilePalette.Hex("#F2EDE4"),
                SecondaryText = TilePalette.Hex("#F9F6F2"),
                Panel = TilePalette.Hex("#262420"),
                Accent = TilePalette.Hex("#B79A7E"),
                AccentText = TilePalette.Hex("#1B1A17")
            };
        }
    }
}
