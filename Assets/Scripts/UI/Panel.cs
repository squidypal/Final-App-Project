using UnityEngine;

namespace Game2048.UI
{
    public abstract class Panel : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup group;

        protected virtual void Awake()
        {
            if (group == null)
            {
                group = GetComponent<CanvasGroup>();
            }
            ApplyVisibility(false);
        }

        public bool IsVisible => group != null && group.blocksRaycasts;

        public void SetVisible(bool visible)
        {
            ApplyVisibility(visible);
        }

        private void ApplyVisibility(bool visible)
        {
            if (group == null)
            {
                return;
            }
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
