using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Game2048.Board;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Game2048.Input
{
    public sealed class SwipeDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<Direction> Swiped;

        private Vector2 pressPosition;
        private bool pressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
            pressPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!pressed)
            {
                return;
            }
            pressed = false;

            Vector2 delta = eventData.position - pressPosition;
            float threshold = Mathf.Max(20f, Mathf.Min(Screen.width, Screen.height) * 0.04f);
            if (delta.magnitude < threshold)
            {
                return;
            }

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                Raise(delta.x > 0f ? Direction.Right : Direction.Left);
            }
            else
            {
                Raise(delta.y > 0f ? Direction.Up : Direction.Down);
            }
        }

#if ENABLE_INPUT_SYSTEM
        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                Raise(Direction.Up);
            }
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                Raise(Direction.Down);
            }
            else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
            {
                Raise(Direction.Left);
            }
            else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
            {
                Raise(Direction.Right);
            }
        }
#endif

        private void Raise(Direction direction)
        {
            Swiped?.Invoke(direction);
        }
    }
}
