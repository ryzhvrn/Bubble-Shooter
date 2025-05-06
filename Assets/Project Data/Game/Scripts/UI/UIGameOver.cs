using System;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BubbleShooter;

namespace Watermelon
{
    public class UIGameOver : UIPage
    {
        [Header("Settings")]
        [SerializeField] float replayDelay;
        [SerializeField] Color activeButtonColor;
        [SerializeField] Color activeButtonShadowColor;
        [SerializeField] Color disableButtonColor;
        [SerializeField] Color disableButtonShadowColor;
        [SerializeField] Sprite activeAdSprite;
        [SerializeField] Sprite disableAdSprite;

        [SerializeField] UIScalableObject levelFailed;

        [SerializeField] UIFade backgroundFade;

        [SerializeField] UIScalableObject skipButtonRect;
        [SerializeField] Button skipButton;
        [SerializeField] Image skipButtonImage;
        [SerializeField] Image skipAdImage;


        [Header("No Thanks Label")]
        [SerializeField] Button replayButton;
        [SerializeField] Text replayText;

        private TweenCase skipPingPongCase;
        private Shadow skipButtonShadow;

        //private bool IsAdAndRewardedVideoEnabled => (AdsManager.IsForcedAdEnabled() && AdsManager.IsRewardBasedVideoLoaded());
        private bool IsAdAndRewardedVideoEnabled = true;
        private bool closePagePressed;

        public override void Init()
        {
            base.Init();

            skipButtonShadow = skipButtonImage.gameObject.GetComponent<Shadow>();
        }

        #region Show/Hide
        
        public override void Show()
        {
            if (isPageDisplayed)
                return;

            isPageDisplayed = true;
            canvas.enabled = true;
            closePagePressed = false;

            // RESET

            levelFailed.Hide(immediately: true);
            skipButtonRect.Hide(immediately: true);
            HideReplayButton();

            UpdateSkipButton();

            //

            float fadeDuration = 0.3f;
            backgroundFade.Show(false, fadeDuration);

            Tween.DelayedCall(fadeDuration * 0.8f, delegate { 
            
                levelFailed.Show(false, scaleMultiplier: 1.1f);
                
                ShowReplayButton(replayDelay, immediately: !IsAdAndRewardedVideoEnabled);

                skipButtonRect.Show(false, scaleMultiplier: 1.05f, duration: 0.5f, onCompleted: delegate {

                    if (IsAdAndRewardedVideoEnabled)
                        skipPingPongCase = skipButtonRect.RectTransform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);
                });                                
            });
        }

        public override void ShowImmediately()
        {
            if (isPageDisplayed)
                return;

            isPageDisplayed = true;
            canvas.enabled = true;
            closePagePressed = false;

            UpdateSkipButton();

            backgroundFade.Show(immediately: true);
            levelFailed.Show(immediately: true);

            ShowReplayButton(replayDelay * 0.5f, immediately: false);

            skipButtonRect.Show(true);
            if (IsAdAndRewardedVideoEnabled) skipPingPongCase = skipButtonRect.RectTransform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);
        }

        public override void Hide()
        {
            if (!isPageDisplayed)
                return;

            backgroundFade.Hide(false, 0.3f);

            Tween.DelayedCall(0.3f, delegate {

                canvas.enabled = false;
                isPageDisplayed = false;

                if (skipPingPongCase != null && skipPingPongCase.isActive) skipPingPongCase.Kill();
            });
        }

        public override void HideImmediately()
        {
            if (!isPageDisplayed)
                return;

            canvas.enabled = false;
            isPageDisplayed = false;
            
            if (skipPingPongCase != null && skipPingPongCase.isActive) skipPingPongCase.Kill();
        }

        #endregion

        #region Replay Block

        public void ShowReplayButton(float delayToShow = 0.3f, bool immediately = true)
        {
            if (immediately)
            {
                replayButton.gameObject.SetActive(true);
                replayText.gameObject.SetActive(true);

                return;
            }

            Tween.DelayedCall(delayToShow, delegate { 

                replayButton.gameObject.SetActive(true);
                replayText.gameObject.SetActive(true);

            });
        }

        public void HideReplayButton()
        {
            replayButton.gameObject.SetActive(false);
            replayText.gameObject.SetActive(false);
        }

        #endregion

        private void UpdateSkipButton()
        {
            skipButton.interactable = IsAdAndRewardedVideoEnabled;
            skipButtonImage.color = IsAdAndRewardedVideoEnabled ? activeButtonColor : disableButtonColor;
            skipButtonShadow.effectColor = IsAdAndRewardedVideoEnabled ? activeButtonShadowColor : disableButtonShadowColor;
            skipAdImage.sprite = IsAdAndRewardedVideoEnabled ? activeAdSprite : disableAdSprite;
        }

        #region Buttons 

        public void SkipButton()
        {
            if (closePagePressed)
                return;

            closePagePressed = true;

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            GameController.OnSkipLevelPressed();
        }

        public void ReplayButton()
        {
            if (closePagePressed)
                return;

            closePagePressed = true;

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            GameController.OnGameOverPageClosed();
        }

        #endregion
    }
}