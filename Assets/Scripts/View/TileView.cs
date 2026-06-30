using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game2048.Services;

namespace Game2048.View
{
    public sealed class TileView : MonoBehaviour
    {
        public int TileId { get; private set; }
        public int Row { get; private set; }
        public int Col { get; private set; }

        private RectTransform rectTransform;
        private Image background;
        private TextMeshProUGUI label;
        private Coroutine activeMove;
        private Coroutine activePop;
        private float cellSize = 100f;

        public void Build(RectTransform parent, Sprite sprite)
        {
            rectTransform = (RectTransform)transform;
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            background = gameObject.AddComponent<Image>();
            background.sprite = sprite;
            background.type = Image.Type.Sliced;
            background.raycastTarget = false;

            var labelObject = new GameObject("Label", typeof(RectTransform));
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.SetParent(rectTransform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            label = labelObject.AddComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
        }

        public void SetCellSize(float size)
        {
            cellSize = size;
            rectTransform.sizeDelta = new Vector2(size, size);
        }

        public void SetValue(int value)
        {
            var style = TilePalette.StyleFor(value);
            background.color = style.Background;
            label.text = value.ToString();
            label.color = style.TextColor;
            label.fontSize = cellSize * style.FontScale;
        }

        public void PlaceInstant(int row, int col, Vector2 position)
        {
            Row = row;
            Col = col;
            CancelMove();
            rectTransform.anchoredPosition = position;
            rectTransform.localScale = Vector3.one;
        }

        public void AssignId(int id)
        {
            TileId = id;
        }

        public void MoveTo(int row, int col, Vector2 position, float duration)
        {
            Row = row;
            Col = col;
            CancelMove();
            if (duration <= 0f || !isActiveAndEnabled)
            {
                rectTransform.anchoredPosition = position;
                return;
            }
            activeMove = StartCoroutine(MoveRoutine(position, duration));
        }

        public void PlaySpawn(float duration)
        {
            CancelPop();
            if (duration <= 0f || !isActiveAndEnabled)
            {
                rectTransform.localScale = Vector3.one;
                return;
            }
            rectTransform.localScale = Vector3.zero;
            activePop = StartCoroutine(ScaleRoutine(0f, 1f, duration));
        }

        public void PlayMergePop(float duration)
        {
            CancelPop();
            if (duration <= 0f || !isActiveAndEnabled)
            {
                rectTransform.localScale = Vector3.one;
                return;
            }
            activePop = StartCoroutine(PopRoutine(duration));
        }

        public void ResetForPool()
        {
            CancelMove();
            CancelPop();
            rectTransform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        private IEnumerator MoveRoutine(Vector2 target, float duration)
        {
            Vector2 start = rectTransform.anchoredPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, target, t);
                yield return null;
            }
            rectTransform.anchoredPosition = target;
            activeMove = null;
        }

        private IEnumerator ScaleRoutine(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                float scale = Mathf.LerpUnclamped(from, to, t);
                rectTransform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            rectTransform.localScale = Vector3.one;
            activePop = null;
        }

        private IEnumerator PopRoutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = 1f + 0.18f * Mathf.Sin(t * Mathf.PI);
                rectTransform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            rectTransform.localScale = Vector3.one;
            activePop = null;
        }

        private void CancelMove()
        {
            if (activeMove != null)
            {
                StopCoroutine(activeMove);
                activeMove = null;
            }
        }

        private void CancelPop()
        {
            if (activePop != null)
            {
                StopCoroutine(activePop);
                activePop = null;
            }
        }
    }
}
