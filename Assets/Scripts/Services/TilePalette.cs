using UnityEngine;

namespace Game2048.Services
{
    public struct TileStyle
    {
        public Color Background;
        public Color TextColor;
        public float FontScale;
    }

    public static class TilePalette
    {
        private static readonly Color DarkText = Hex("#776E65");
        private static readonly Color LightText = Hex("#F9F6F2");
        private static readonly Color SuperTile = Hex("#3C3A32");

        public static TileStyle StyleFor(int value)
        {
            return new TileStyle
            {
                Background = BackgroundFor(value),
                TextColor = value <= 4 ? DarkText : LightText,
                FontScale = FontScaleFor(value)
            };
        }

        private static Color BackgroundFor(int value)
        {
            switch (value)
            {
                case 2: return Hex("#EEE4DA");
                case 4: return Hex("#EDE0C8");
                case 8: return Hex("#F2B179");
                case 16: return Hex("#F59563");
                case 32: return Hex("#F67C5F");
                case 64: return Hex("#F65E3B");
                case 128: return Hex("#EDCF72");
                case 256: return Hex("#EDCC61");
                case 512: return Hex("#EDC850");
                case 1024: return Hex("#EDC53F");
                case 2048: return Hex("#EDC22E");
                default: return SuperTile;
            }
        }

        private static float FontScaleFor(int value)
        {
            if (value < 100) return 0.46f;
            if (value < 1000) return 0.38f;
            if (value < 10000) return 0.30f;
            return 0.24f;
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
