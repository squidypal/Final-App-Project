using UnityEngine;

namespace Game2048.Utilities
{
    public static class RoundedSprite
    {
        private static Sprite cached;

        public static Sprite Get()
        {
            if (cached == null)
            {
                cached = Create(48, 12);
            }
            return cached;
        }

        public static Sprite Create(int size, int radius)
        {
            var texture = BuildTexture(size, radius);
            var border = new Vector4(radius, radius, radius, radius);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        public static Texture2D BuildTexture(int size, int radius)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = Coverage(x, y, size, radius);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static float Coverage(int x, int y, int size, int radius)
        {
            float fx = x + 0.5f;
            float fy = y + 0.5f;
            float cx = Mathf.Clamp(fx, radius, size - radius);
            float cy = Mathf.Clamp(fy, radius, size - radius);
            float dx = fx - cx;
            float dy = fy - cy;
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01(radius - distance + 0.5f);
        }
    }
}
