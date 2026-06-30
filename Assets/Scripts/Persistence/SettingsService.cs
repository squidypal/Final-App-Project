using System;
using UnityEngine;

namespace Game2048.Persistence
{
    public enum ThemeMode
    {
        Light = 0,
        Dark = 1
    }

    public sealed class SettingsService
    {
        private const string SoundKey = "settings.sound";
        private const string HapticsKey = "settings.haptics";
        private const string AnimationsKey = "settings.animations";
        private const string ThemeKey = "settings.theme";

        public event Action Changed;

        public bool SoundEnabled
        {
            get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
            set => SetBool(SoundKey, value);
        }

        public bool HapticsEnabled
        {
            get => PlayerPrefs.GetInt(HapticsKey, 1) == 1;
            set => SetBool(HapticsKey, value);
        }

        public bool AnimationsEnabled
        {
            get => PlayerPrefs.GetInt(AnimationsKey, 1) == 1;
            set => SetBool(AnimationsKey, value);
        }

        public ThemeMode Theme
        {
            get => (ThemeMode)PlayerPrefs.GetInt(ThemeKey, (int)ThemeMode.Light);
            set
            {
                PlayerPrefs.SetInt(ThemeKey, (int)value);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        private void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
