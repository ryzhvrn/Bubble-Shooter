using UnityEngine;

namespace Watermelon.BubbleShooter
{
    [RequireComponent(typeof(Canvas))]
    public abstract class UIPage : MonoBehaviour
    {
        protected bool isPageDisplayed;
        public bool IsPageDisplayed => isPageDisplayed;

        protected Canvas canvas;
        public Canvas Canvas => canvas;

        public virtual void Init()
        {
            canvas = GetComponent<Canvas>();
        }

        public abstract void Show();
        public abstract void Hide();
        public abstract void ShowImmediately();
        public abstract void HideImmediately();
    }
}