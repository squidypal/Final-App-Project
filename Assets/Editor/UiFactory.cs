using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game2048.UI;

namespace Game2048.EditorTools
{
    public static class UiFactory
    {
        public static Sprite RoundedSprite;

        public static RectTransform Create(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
        }

        public static void FullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static Image Img(GameObject go, Color color, bool raycast)
        {
            var image = go.AddComponent<Image>();
            image.sprite = RoundedSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public static TextMeshProUGUI Txt(RectTransform rt, string text, float fontSize, Color color, TextAlignmentOptions align, FontStyles style = FontStyles.Normal)
        {
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = align;
            label.fontStyle = style;
            label.raycastTarget = false;
            return label;
        }

        public static Button Btn(RectTransform rt, string label, float fontSize)
        {
            var image = Img(rt.gameObject, Color.white, true);
            Themed(rt.gameObject, ThemeRole.AccentFill);

            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.fadeDuration = 0.08f;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            button.colors = colors;

            var labelRect = Create("Label", rt);
            FullStretch(labelRect);
            Txt(labelRect, label, fontSize, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            Themed(labelRect.gameObject, ThemeRole.AccentText);
            return button;
        }

        public static Toggle Tgl(RectTransform rt)
        {
            var background = Img(rt.gameObject, Hex("#CDC1B4"), true);

            var toggle = rt.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.transition = Selectable.Transition.ColorTint;

            var checkRect = Create("Checkmark", rt);
            Place(checkRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, rt.sizeDelta * 0.55f);
            var check = Img(checkRect.gameObject, Hex("#8F7A66"), false);
            toggle.graphic = check;
            return toggle;
        }

        public static ThemedElement Themed(GameObject go, ThemeRole role)
        {
            var element = go.AddComponent<ThemedElement>();
            Wire(element, "role", role);
            return element;
        }

        public static void Wire(UnityEngine.Object target, string field, object value)
        {
            var type = target.GetType();
            FieldInfo info = null;
            while (type != null && info == null)
            {
                info = type.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                type = type.BaseType;
            }
            if (info == null)
            {
                Debug.LogError($"[UiFactory] Field '{field}' not found on {target.GetType().Name}");
                return;
            }
            info.SetValue(target, value);
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
