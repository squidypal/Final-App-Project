using UnityEngine;
using Game2048.Persistence;
using Game2048.Services;
using Game2048.View;
using Game2048.Input;
using Game2048.UI;

namespace Game2048.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameController gameController;
        [SerializeField] private BoardView boardView;
        [SerializeField] private SwipeDetector swipeDetector;
        [SerializeField] private HUDController hud;
        [SerializeField] private GameOverPanel gameOverPanel;
        [SerializeField] private WinPanel winPanel;
        [SerializeField] private SettingsPanel settingsPanel;

        public SettingsService Settings { get; private set; }
        public ThemeService Theme { get; private set; }
        public ScoreService Score { get; private set; }
        public StatsService Stats { get; private set; }
        public SaveSystem Save { get; private set; }
        public AudioService Audio { get; private set; }
        public GameData Data { get; private set; }

        public GameController Game => gameController;
        public BoardView BoardView => boardView;
        public SwipeDetector Swipe => swipeDetector;
        public GameOverPanel GameOverPanel => gameOverPanel;
        public WinPanel WinPanel => winPanel;
        public SettingsPanel SettingsPanel => settingsPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Settings = new SettingsService();
            Theme = new ThemeService(Settings);
            Score = new ScoreService();
            Stats = new StatsService();
            Save = new SaveSystem();

            Audio = gameObject.AddComponent<AudioService>();
            Audio.Initialize(Settings);

            Data = Save.Load();
            Stats.Bind(Data.stats);
            Score.Initialize(Data.score, Data.bestScore);
        }

        private void Start()
        {
            gameController.Begin(Data);
        }

        public void Persist()
        {
            Save.Save(Data);
        }

        public void ResetProgress()
        {
            Save.Delete();
            Data.stats = new StatsData();
            Stats.Bind(Data.stats);
            Score.Initialize(0, 0);
            Data.score = 0;
            Data.bestScore = 0;
            Data.highestValue = 0;
            Data.reachedTarget = false;
            Data.hasActiveGame = false;
            gameController.NewGame();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && gameController != null)
            {
                gameController.FlushSave();
            }
        }

        private void OnApplicationQuit()
        {
            if (gameController != null)
            {
                gameController.FlushSave();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
