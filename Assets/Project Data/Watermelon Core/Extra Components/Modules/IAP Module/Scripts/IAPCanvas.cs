using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class IAPCanvas : MonoBehaviour
    {
        private static IAPCanvas iapCanvas;

        [Header("Message")]
        [SerializeField] GameObject messagePanel;
        [SerializeField] Text messageText;

        [Header("Loading")]
        [SerializeField] GameObject loadingContainer;
        [SerializeField] Transform loadingPanel;
        [SerializeField] Text errorMessageText;

        [Space]
        [SerializeField] Animator circleAnimation;

        private TweenCase messageScaleTweenCase;
        private TweenCase loadingScaleTweenCase;

        public void Init()
        {
            iapCanvas = this;
        }

        public static void ShowMessage(string message)
        {
            if (iapCanvas.messageScaleTweenCase != null && !iapCanvas.messageScaleTweenCase.isCompleted)
                iapCanvas.messageScaleTweenCase.Kill();

            iapCanvas.messagePanel.SetActive(true);
            iapCanvas.messagePanel.transform.localScale = Vector3.zero;
            iapCanvas.messageScaleTweenCase = iapCanvas.messagePanel.transform.DOScale(1.0f, 0.4f, unscaledTime: true).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
            {
                iapCanvas.messageScaleTweenCase = Tween.DelayedCall(0.3f, delegate
                {
                    iapCanvas.messageScaleTweenCase = iapCanvas.messagePanel.transform.DOScale(0.0f, 0.2f, unscaledTime: true).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
                    {
                        iapCanvas.messagePanel.SetActive(false);
                    });
                }, unscaledTime: true);
            });

            iapCanvas.messageText.text = message;
        }

        public static void ShowLoadingPanel()
        {
            if (iapCanvas.loadingScaleTweenCase != null && !iapCanvas.loadingScaleTweenCase.isCompleted)
                iapCanvas.loadingScaleTweenCase.Kill();

            iapCanvas.circleAnimation.enabled = true;

            iapCanvas.loadingContainer.SetActive(true);
            iapCanvas.loadingPanel.transform.localScale = Vector3.zero;
            iapCanvas.loadingScaleTweenCase = iapCanvas.loadingPanel.transform.DOScale(1.0f, 0.4f, unscaledTime: true).SetEasing(Ease.Type.CubicOut);
        }

        public static void ChangeLoadingMessage(string message)
        {
            iapCanvas.errorMessageText.text = message;
        }

        public static void HideLoadingPanel()
        {
            if (iapCanvas.loadingScaleTweenCase != null && !iapCanvas.loadingScaleTweenCase.isCompleted)
                iapCanvas.loadingScaleTweenCase.Kill();

            iapCanvas.circleAnimation.enabled = false;

            iapCanvas.loadingScaleTweenCase = iapCanvas.loadingPanel.transform.DOScale(0.0f, 0.2f, unscaledTime: true).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
            {
                iapCanvas.loadingContainer.SetActive(false);
            });
        }
    }
}
