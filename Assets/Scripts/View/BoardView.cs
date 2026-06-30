using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game2048.Board;
using Game2048.Persistence;
using Game2048.Services;
using Game2048.Utilities;

namespace Game2048.View
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private Sprite roundedSprite;
        [SerializeField] private float spacingFraction = 0.03f;

        private const float SlideDuration = 0.12f;
        private const float SpawnDuration = 0.10f;
        private const float MergeDuration = 0.14f;

        private RectTransform rectTransform;
        private Image boardBackground;
        private RectTransform cellsContainer;
        private RectTransform tilesContainer;

        private ThemeService theme;
        private SettingsService settings;

        private int boardSize = 4;
        private float cellSize = 100f;
        private float spacing = 8f;

        private readonly List<Image> cells = new List<Image>();
        private readonly Dictionary<int, TileView> activeTiles = new Dictionary<int, TileView>();
        private readonly Stack<TileView> pool = new Stack<TileView>();
        private bool initialized;

        public bool IsAnimating { get; private set; }

        public void Initialize(int size, ThemeService themeService, SettingsService settingsService)
        {
            boardSize = size;
            theme = themeService;
            settings = settingsService;

            rectTransform = (RectTransform)transform;
            if (roundedSprite == null)
            {
                roundedSprite = RoundedSprite.Get();
            }

            boardBackground = GetComponent<Image>();
            if (boardBackground == null)
            {
                boardBackground = gameObject.AddComponent<Image>();
                boardBackground.sprite = roundedSprite;
                boardBackground.type = Image.Type.Sliced;
            }

            cellsContainer = CreateContainer("Cells");
            tilesContainer = CreateContainer("Tiles");

            BuildCells();
            Recompute();
            ApplyTheme();
            initialized = true;
        }

        public void ApplyTheme()
        {
            if (theme == null)
            {
                return;
            }
            var palette = theme.Current;
            if (boardBackground != null)
            {
                boardBackground.color = palette.BoardBackground;
            }
            foreach (var cell in cells)
            {
                cell.color = palette.EmptyCell;
            }
            foreach (var tile in activeTiles.Values)
            {
                tile.SetValue(ValueOf(tile));
            }
        }

        public void BuildInstant(IEnumerable<Tile> tiles)
        {
            ClearAll();
            foreach (var tile in tiles)
            {
                var view = GetTile();
                view.AssignId(tile.Id);
                view.SetCellSize(cellSize);
                view.SetValue(tile.Value);
                view.PlaceInstant(tile.Row, tile.Col, PositionFor(tile.Row, tile.Col));
                tileValues[tile.Id] = tile.Value;
                activeTiles[tile.Id] = view;
            }
        }

        public void ApplyMove(MoveResult result, Action onComplete)
        {
            StartCoroutine(ApplyMoveRoutine(result, onComplete));
        }

        private IEnumerator ApplyMoveRoutine(MoveResult result, Action onComplete)
        {
            IsAnimating = true;
            bool animate = settings == null || settings.AnimationsEnabled;
            float slide = animate ? SlideDuration : 0f;

            foreach (var step in result.Slides)
            {
                if (activeTiles.TryGetValue(step.Id, out var view))
                {
                    view.MoveTo(step.Row, step.Col, PositionFor(step.Row, step.Col), slide);
                }
            }

            foreach (var merge in result.Merges)
            {
                Vector2 destination = PositionFor(merge.Row, merge.Col);
                if (activeTiles.TryGetValue(merge.SurvivingId, out var survivor))
                {
                    survivor.MoveTo(merge.Row, merge.Col, destination, slide);
                }
                if (activeTiles.TryGetValue(merge.AbsorbedId, out var absorbed))
                {
                    absorbed.MoveTo(merge.Row, merge.Col, destination, slide);
                }
            }

            if (slide > 0f)
            {
                yield return new WaitForSeconds(slide);
            }

            foreach (var merge in result.Merges)
            {
                if (activeTiles.TryGetValue(merge.AbsorbedId, out var absorbed))
                {
                    activeTiles.Remove(merge.AbsorbedId);
                    tileValues.Remove(merge.AbsorbedId);
                    ReturnTile(absorbed);
                }
                if (activeTiles.TryGetValue(merge.SurvivingId, out var survivor))
                {
                    survivor.SetValue(merge.NewValue);
                    tileValues[merge.SurvivingId] = merge.NewValue;
                    survivor.PlayMergePop(animate ? MergeDuration : 0f);
                }
            }

            if (result.HasSpawn)
            {
                var view = GetTile();
                view.AssignId(result.Spawn.Id);
                view.SetCellSize(cellSize);
                view.SetValue(result.Spawn.Value);
                view.PlaceInstant(result.Spawn.Row, result.Spawn.Col, PositionFor(result.Spawn.Row, result.Spawn.Col));
                view.PlaySpawn(animate ? SpawnDuration : 0f);
                activeTiles[result.Spawn.Id] = view;
                tileValues[result.Spawn.Id] = result.Spawn.Value;
            }

            if (animate)
            {
                yield return new WaitForSeconds(SpawnDuration);
            }

            IsAnimating = false;
            onComplete?.Invoke();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (initialized)
            {
                Recompute();
            }
        }

        private void Recompute()
        {
            float boardWidth = rectTransform.rect.width;
            if (boardWidth <= 0f)
            {
                return;
            }
            spacing = boardWidth * spacingFraction;
            cellSize = (boardWidth - spacing * (boardSize + 1)) / boardSize;

            for (int i = 0; i < cells.Count; i++)
            {
                int row = i / boardSize;
                int col = i % boardSize;
                var cellRect = (RectTransform)cells[i].transform;
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                cellRect.anchoredPosition = PositionFor(row, col);
            }

            foreach (var tile in activeTiles.Values)
            {
                tile.SetCellSize(cellSize);
                tile.SetValue(ValueOf(tile));
                tile.PlaceInstant(tile.Row, tile.Col, PositionFor(tile.Row, tile.Col));
            }
        }

        private Vector2 PositionFor(int row, int col)
        {
            float boardWidth = rectTransform.rect.width;
            float origin = -boardWidth * 0.5f + spacing + cellSize * 0.5f;
            float step = cellSize + spacing;
            float x = origin + col * step;
            float y = -(origin + row * step);
            return new Vector2(x, y);
        }

        private void BuildCells()
        {
            int count = boardSize * boardSize;
            for (int i = 0; i < count; i++)
            {
                var cellObject = new GameObject($"Cell_{i}", typeof(RectTransform));
                var cellRect = (RectTransform)cellObject.transform;
                cellRect.SetParent(cellsContainer, false);
                cellRect.anchorMin = new Vector2(0.5f, 0.5f);
                cellRect.anchorMax = new Vector2(0.5f, 0.5f);
                cellRect.pivot = new Vector2(0.5f, 0.5f);

                var image = cellObject.AddComponent<Image>();
                image.sprite = roundedSprite;
                image.type = Image.Type.Sliced;
                image.raycastTarget = false;
                cells.Add(image);
            }
        }

        private RectTransform CreateContainer(string name)
        {
            var container = new GameObject(name, typeof(RectTransform));
            var containerRect = (RectTransform)container.transform;
            containerRect.SetParent(rectTransform, false);
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            return containerRect;
        }

        private readonly Dictionary<int, int> tileValues = new Dictionary<int, int>();

        private int ValueOf(TileView tile)
        {
            return tileValues.TryGetValue(tile.TileId, out var value) ? value : 2;
        }

        private TileView GetTile()
        {
            TileView view;
            if (pool.Count > 0)
            {
                view = pool.Pop();
                view.Activate();
            }
            else
            {
                var go = new GameObject("Tile", typeof(RectTransform));
                view = go.AddComponent<TileView>();
                view.Build(tilesContainer, roundedSprite);
            }
            return view;
        }

        private void ReturnTile(TileView view)
        {
            view.ResetForPool();
            pool.Push(view);
        }

        private void ClearAll()
        {
            foreach (var tile in activeTiles.Values)
            {
                ReturnTile(tile);
            }
            activeTiles.Clear();
            tileValues.Clear();
        }
    }
}
