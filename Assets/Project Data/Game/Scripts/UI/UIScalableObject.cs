using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class UIScalableObject
    {       
        [SerializeField] RectTransform rect;

        public RectTransform RectTransform => rect;   

        /// <summary>
        /// Reset object to the started scale
        /// </summary>
        public void Reset()
        {
            rect.localScale = Vector3.one;
        }

        public void Show(bool immediately = true, float scaleMultiplier = 1.05f, float duration = 0.5f, Action onCompleted = null)
        {
            if (immediately)
            {
                rect.localScale = Vector3.one;
                onCompleted?.Invoke();
                return;
            }


            if (rect.localScale == Vector3.one)
            {
                onCompleted?.Invoke();
                return;
            }

            // reset
            rect.localScale = Vector3.zero;

            rect.DOPushScale(Vector3.one * scaleMultiplier, Vector3.one, duration * 0.64f, duration * 0.36f, Ease.Type.CubicOut, Ease.Type.CubicIn).OnComplete(delegate {
                onCompleted?.Invoke();
            });
        }

        public void Hide(bool immediately = true, float scaleMultiplier = 1.05f, float duration = 0.5f, Action onCompleted = null)
        {
            if (immediately)
            {
                rect.localScale = Vector3.zero;
                onCompleted?.Invoke();
                return;
            }

            if (rect.localScale == Vector3.zero)
            {
                onCompleted?.Invoke();
                return;
            }

            rect.DOPushScale(Vector3.one * scaleMultiplier, Vector3.zero, duration * 0.36f, duration * 0.64f, Ease.Type.CubicOut, Ease.Type.CubicIn).OnComplete(delegate {
                onCompleted?.Invoke();
            });
        }
    }

}