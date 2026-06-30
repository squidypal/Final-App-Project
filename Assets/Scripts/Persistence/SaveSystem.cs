using System;
using System.IO;
using UnityEngine;

namespace Game2048.Persistence
{
    public sealed class SaveSystem
    {
        private const string FileName = "save.json";
        private readonly string filePath;
        private readonly string tempPath;

        public SaveSystem()
        {
            filePath = Path.Combine(Application.persistentDataPath, FileName);
            tempPath = filePath + ".tmp";
        }

        public GameData Load()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new GameData();
                }

                string json = File.ReadAllText(filePath);
                var data = JsonUtility.FromJson<GameData>(json);
                if (data == null)
                {
                    return new GameData();
                }
                data.stats ??= new StatsData();
                data.cells ??= new int[0];
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveSystem] Failed to load save, starting fresh: {exception.Message}");
                return new GameData();
            }
        }

        public void Save(GameData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                File.WriteAllText(tempPath, json);

                if (File.Exists(filePath))
                {
                    File.Replace(tempPath, filePath, null);
                }
                else
                {
                    File.Move(tempPath, filePath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveSystem] Failed to save: {exception.Message}");
            }
        }

        public void Delete()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveSystem] Failed to delete save: {exception.Message}");
            }
        }
    }
}
