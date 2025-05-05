using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class UIFade
    {
        [SerializeField] CanvasGroup fadeCanvasGroup;

        public void Show(bool immediately = false, float duration = 0.4f)
        {
            if (immediately)
            {
                fadeCanvasGroup.alpha = 1f;
                return;
            }

            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.DOFade(1f, duration, 0, true);
        }

        public void Hide(bool immediately = false, float duration = 0.4f)
        {
            if (immediately)
            {
                fadeCanvasGroup.alpha = 0f;
                return;
            }

            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.DOFade(0, duration, 0, true);
        }
    }
}
