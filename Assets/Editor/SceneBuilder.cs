using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Game2048.Core;
using Game2048.Input;
using Game2048.UI;
using Game2048.Utilities;
using Game2048.View;

namespace Game2048.EditorTools
{
    public static class SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Game.unity";
        private const string SpritePath = "Assets/Art/RoundedRect.png";
        private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        private static readonly Color PrimaryColor = Hex("#776E65");
        private static readonly Color SecondaryColor = Hex("#F9F6F2");
        private static readonly Color AccentColor = Hex("#8F7A66");
        private static readonly Color PanelColor = Hex("#FAF8EF");
        private static readonly Color BoardColor = Hex("#BBADA0");

        [MenuItem("Tools/2048/Build Game Scene", priority = 0)]
        public static void BuildScene()
        {
            if (!EnsureTmpEssentials())
            {
                return;
            }

            var sprite = GenerateRoundedSpriteAsset();
            UiFactory.RoundedSprite = sprite;
            ConfigurePlayerSettings();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem));
            var inputModule = eventSystemGo.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var background = UiFactory.Create("Background", canvasGo.transform);
            UiFactory.FullStretch(background);
            UiFactory.Img(background.gameObject, PanelColor, false);
            UiFactory.Themed(background.gameObject, ThemeRole.Background);

            var safe = UiFactory.Create("SafeArea", canvasGo.transform);
            UiFactory.FullStretch(safe);
            safe.gameObject.AddComponent<SafeAreaFitter>();

            var title = UiFactory.Create("Title", safe);
            UiFactory.Place(title, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -30), new Vector2(440, 150));
            UiFactory.Txt(title, "2048", 120f, PrimaryColor, TextAlignmentOptions.Left, FontStyles.Bold);
            UiFactory.Themed(title.gameObject, ThemeRole.PrimaryText);

            var bestValue = CreateStatBox(safe, "BEST", new Vector2(-40, -40));
            var scoreValue = CreateStatBox(safe, "SCORE", new Vector2(-290, -40));

            var newGameRt = UiFactory.Create("NewGameButton", safe);
            UiFactory.Place(newGameRt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -300), new Vector2(360, 120));
            var newGameButton = UiFactory.Btn(newGameRt, "NEW GAME", 40f);

            var undoRt = UiFactory.Create("UndoButton", safe);
            UiFactory.Place(undoRt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(430, -300), new Vector2(280, 120));
            var undoButton = UiFactory.Btn(undoRt, "UNDO", 40f);

            var menuRt = UiFactory.Create("MenuButton", safe);
            UiFactory.Place(menuRt, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -300), new Vector2(280, 120));
            var menuButton = UiFactory.Btn(menuRt, "MENU", 40f);

            var board = UiFactory.Create("BoardArea", safe);
            board.anchorMin = new Vector2(0, 1);
            board.anchorMax = new Vector2(1, 1);
            board.pivot = new Vector2(0.5f, 1f);
            board.sizeDelta = new Vector2(-80, 1000);
            board.anchoredPosition = new Vector2(0, -460);
            var boardFitter = board.gameObject.AddComponent<AspectRatioFitter>();
            boardFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            boardFitter.aspectRatio = 1f;
            UiFactory.Img(board.gameObject, BoardColor, false);
            var boardView = board.gameObject.AddComponent<BoardView>();
            UiFactory.Wire(boardView, "roundedSprite", sprite);

            var catcher = UiFactory.Create("SwipeCatcher", board);
            UiFactory.FullStretch(catcher);
            UiFactory.Img(catcher.gameObject, new Color(0, 0, 0, 0), true);
            var swipe = catcher.gameObject.AddComponent<SwipeDetector>();

            var footer = UiFactory.Create("Footer", safe);
            UiFactory.Place(footer, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(-80, 90));
            UiFactory.Txt(footer, "Swipe to move  •  merge tiles to reach 2048", 34f, PrimaryColor, TextAlignmentOptions.Center);
            UiFactory.Themed(footer.gameObject, ThemeRole.PrimaryText);

            var hudGo = UiFactory.Create("HUD", safe);
            var hud = hudGo.gameObject.AddComponent<HUDController>();
            UiFactory.Wire(hud, "scoreValue", scoreValue);
            UiFactory.Wire(hud, "bestValue", bestValue);
            UiFactory.Wire(hud, "newGameButton", newGameButton);
            UiFactory.Wire(hud, "undoButton", undoButton);
            UiFactory.Wire(hud, "settingsButton", menuButton);

            var gameOver = BuildGameOverPanel(canvasGo.transform);
            var win = BuildWinPanel(canvasGo.transform);
            var settings = BuildSettingsPanel(canvasGo.transform);

            var rootGo = new GameObject("GameRoot");
            var controller = rootGo.AddComponent<GameController>();
            var gm = rootGo.AddComponent<GameManager>();
            UiFactory.Wire(gm, "gameController", controller);
            UiFactory.Wire(gm, "boardView", boardView);
            UiFactory.Wire(gm, "swipeDetector", swipe);
            UiFactory.Wire(gm, "hud", hud);
            UiFactory.Wire(gm, "gameOverPanel", gameOver);
            UiFactory.Wire(gm, "winPanel", win);
            UiFactory.Wire(gm, "settingsPanel", settings);

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(ScenePath));
            Debug.Log($"[2048] Scene built and saved to {ScenePath}. Press Play to run.");
        }

        private static TextMeshProUGUI CreateStatBox(RectTransform parent, string header, Vector2 anchoredPos)
        {
            var box = UiFactory.Create(header + "Box", parent);
            UiFactory.Place(box, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), anchoredPos, new Vector2(230, 120));
            UiFactory.Img(box.gameObject, AccentColor, false);
            UiFactory.Themed(box.gameObject, ThemeRole.AccentFill);

            var labelRt = UiFactory.Create("Label", box);
            FillRange(labelRt, new Vector2(0, 0.5f), new Vector2(1, 1));
            UiFactory.Txt(labelRt, header, 30f, SecondaryColor, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Themed(labelRt.gameObject, ThemeRole.SecondaryText);

            var valueRt = UiFactory.Create("Value", box);
            FillRange(valueRt, new Vector2(0, 0), new Vector2(1, 0.55f));
            var value = UiFactory.Txt(valueRt, "0", 52f, SecondaryColor, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Themed(valueRt.gameObject, ThemeRole.SecondaryText);
            return value;
        }

        private static GameOverPanel BuildGameOverPanel(Transform parent)
        {
            var panel = UiFactory.Create("GameOverPanel", parent);
            UiFactory.FullStretch(panel);
            var group = panel.gameObject.AddComponent<CanvasGroup>();
            UiFactory.Img(panel.gameObject, new Color(0, 0, 0, 0.6f), true);
            var controller = panel.gameObject.AddComponent<GameOverPanel>();

            var card = CreateCard(panel, new Vector2(820, 620));

            var titleRt = CardChild(card, new Vector2(0, -60), new Vector2(720, 130));
            UiFactory.Txt(titleRt, "Game Over", 90f, PrimaryColor, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Themed(titleRt.gameObject, ThemeRole.PrimaryText);

            var scoreLabelRt = CardChild(card, new Vector2(0, -220), new Vector2(720, 60));
            UiFactory.Txt(scoreLabelRt, "SCORE", 36f, PrimaryColor, TextAlignmentOptions.Center);
            UiFactory.Themed(scoreLabelRt.gameObject, ThemeRole.PrimaryText);

            var scoreRt = CardChild(card, new Vector2(0, -290), new Vector2(720, 110));
            var finalScore = UiFactory.Txt(scoreRt, "0", 80f, PrimaryColor, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Themed(scoreRt.gameObject, ThemeRole.PrimaryText);

            var buttonRt = CardChild(card, new Vector2(0, -460), new Vector2(520, 120));
            var tryAgain = UiFactory.Btn(buttonRt, "TRY AGAIN", 44f);

            UiFactory.Wire(controller, "group", group);
            UiFactory.Wire(controller, "finalScoreText", finalScore);
            UiFactory.Wire(controller, "tryAgainButton", tryAgain);
            return controller;
        }

        private static WinPanel BuildWinPanel(Transform parent)
        {
            var panel = UiFactory.Create("WinPanel", parent);
            UiFactory.FullStretch(panel);
            var group = panel.gameObject.AddComponent<CanvasGroup>();
            UiFactory.Img(panel.gameObject, new Color(0, 0, 0, 0.6f), true);
            var controller = panel.gameObject.AddComponent<WinPanel>();

            var card = CreateCard(panel, new Vector2(820, 660));

            var titleRt = CardChild(card, new Vector2(0, -60), new Vector2(720, 130));
            UiFactory.Txt(titleRt, "You Win!", 96f, PrimaryColor, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Themed(titleRt.gameObject, ThemeRole.PrimaryText);

            var subtitleRt = CardChild(card, new Vector2(0, -210), new Vector2(720, 70));
            UiFactory.Txt(subtitleRt, "You reached 2048", 40f, PrimaryColor, TextAlignmentOptions.Center);
            UiFactory.Themed(subtitleRt.gameObject, ThemeRole.PrimaryText);

            var keepRt = CardChild(card, new Vector2(0, -360), new Vector2(520, 120));
            var keepGoing = UiFactory.Btn(keepRt, "KEEP GOING", 44f);

            var newRt = CardChild(card, new Vector2(0, -510), new Vector2(520, 120));
            var newGame = UiFactory.Btn(newRt, "NEW GAME", 44f);

            UiFactory.Wire(controller, "group", group);
            UiFactory.Wire(controller, "keepGoingButton", keepGoing);
            UiFactory.Wire(controller, "newGameButton", newGame);
            return controller;
        }

        private static SettingsPanel BuildSettingsPanel(Transform parent)
        {
            var panel = UiFactory.Create("SettingsPanel", parent);
            UiFactory.FullStretch(panel);
            var group = panel.gameObject.AddComponent<CanvasGroup>();
            UiFactory.Img(panel.gameObject, new Color(0, 0, 0, 0.6f), true);
            var controller = panel.gameObject.AddComponent<SettingsPanel>();

            var card = CreateCard(panel, new Vector2(880, 1240));

            var titleRt = CardChild(card, new Vector2(0, -50), new Vector2(760, 110));
            UiFactory.Txt(titleRt, "Settings", 80f, PrimaryColor, TextAlignmentOptions.Center, FontStyles.Bold);
            UiFactory.Themed(titleRt.gameObject, ThemeRole.PrimaryText);

            var soundToggle = CreateToggleRow(card, "Sound", -200);
            var hapticsToggle = CreateToggleRow(card, "Haptics", -300);
            var animationsToggle = CreateToggleRow(card, "Animations", -400);

            var themeRt = CardChild(card, new Vector2(0, -540), new Vector2(720, 110));
            var themeButton = UiFactory.Btn(themeRt, "Theme: Light", 42f);
            var themeLabel = themeRt.GetComponentInChildren<TextMeshProUGUI>();

            var statsRt = CardChild(card, new Vector2(0, -680), new Vector2(720, 300));
            var statsText = UiFactory.Txt(statsRt, "", 38f, PrimaryColor, TextAlignmentOptions.TopLeft);
            UiFactory.Themed(statsRt.gameObject, ThemeRole.PrimaryText);

            var resetRt = CardChild(card, new Vector2(0, -1000), new Vector2(720, 110));
            var resetButton = UiFactory.Btn(resetRt, "RESET PROGRESS", 40f);

            var closeRt = CardChild(card, new Vector2(0, -1130), new Vector2(720, 110));
            var closeButton = UiFactory.Btn(closeRt, "CLOSE", 42f);

            UiFactory.Wire(controller, "group", group);
            UiFactory.Wire(controller, "soundToggle", soundToggle);
            UiFactory.Wire(controller, "hapticsToggle", hapticsToggle);
            UiFactory.Wire(controller, "animationsToggle", animationsToggle);
            UiFactory.Wire(controller, "themeButton", themeButton);
            UiFactory.Wire(controller, "themeLabel", themeLabel);
            UiFactory.Wire(controller, "resetButton", resetButton);
            UiFactory.Wire(controller, "closeButton", closeButton);
            UiFactory.Wire(controller, "statsText", statsText);
            return controller;
        }

        private static Toggle CreateToggleRow(RectTransform card, string label, float y)
        {
            var row = CardChild(card, new Vector2(0, y), new Vector2(720, 80));

            var labelRt = UiFactory.Create("Label", row);
            FillRange(labelRt, new Vector2(0, 0), new Vector2(0.8f, 1));
            UiFactory.Txt(labelRt, label, 44f, PrimaryColor, TextAlignmentOptions.Left);
            UiFactory.Themed(labelRt.gameObject, ThemeRole.PrimaryText);

            var toggleRt = UiFactory.Create("Toggle", row);
            UiFactory.Place(toggleRt, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 0), new Vector2(72, 72));
            return UiFactory.Tgl(toggleRt);
        }

        private static RectTransform CreateCard(RectTransform parent, Vector2 size)
        {
            var card = UiFactory.Create("Card", parent);
            UiFactory.Place(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            UiFactory.Img(card.gameObject, PanelColor, true);
            UiFactory.Themed(card.gameObject, ThemeRole.Panel);
            return card;
        }

        private static RectTransform CardChild(RectTransform card, Vector2 anchoredPos, Vector2 size)
        {
            var child = UiFactory.Create("Element", card);
            UiFactory.Place(child, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), anchoredPos, size);
            return child;
        }

        private static void FillRange(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Sprite GenerateRoundedSpriteAsset()
        {
            Directory.CreateDirectory("Assets/Art");
            if (!File.Exists(SpritePath))
            {
                var texture = RoundedSprite.BuildTexture(64, 16);
                File.WriteAllBytes(SpritePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spriteBorder = new Vector4(16, 16, 16, 16);
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Final App Project";
            PlayerSettings.productName = "Merge 2048";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            try
            {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.finalappproject.merge2048");
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[2048] Android settings skipped: {exception.Message}");
            }
        }

        [MenuItem("Tools/2048/Import TMP Essentials", priority = 1)]
        public static void ImportTmpMenu()
        {
            if (EnsureTmpEssentials())
            {
                Debug.Log("[2048] TMP Essential Resources already present.");
            }
        }

        private static bool EnsureTmpEssentials()
        {
            if (File.Exists(TmpSettingsPath))
            {
                return true;
            }

            string package = FindTmpEssentialsPackage();
            if (package == null)
            {
                Debug.LogError("[2048] Could not locate 'TMP Essential Resources.unitypackage'. Import it via Window > TextMeshPro > Import TMP Essential Resources, then run Build Game Scene again.");
                return false;
            }

            AssetDatabase.importPackageCompleted -= OnTmpImported;
            AssetDatabase.importPackageCompleted += OnTmpImported;
            AssetDatabase.ImportPackage(package, false);
            Debug.Log("[2048] Importing TMP Essential Resources... the scene will build automatically when import completes.");
            return false;
        }

        private static void OnTmpImported(string packageName)
        {
            AssetDatabase.importPackageCompleted -= OnTmpImported;
            EditorApplication.delayCall += BuildScene;
        }

        private static string FindTmpEssentialsPackage()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            foreach (var folder in new[] { "Library/PackageCache", "Packages" })
            {
                string search = Path.Combine(root, folder);
                if (!Directory.Exists(search))
                {
                    continue;
                }
                var matches = Directory.GetFiles(search, "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
                if (matches.Length > 0)
                {
                    return matches[0];
                }
            }
            return null;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
