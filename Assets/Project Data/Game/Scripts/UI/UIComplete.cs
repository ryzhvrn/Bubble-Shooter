using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using Watermelon.BubbleShooter;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        [Header("Settings")]
        [SerializeField] float continueButtonAppearDelay;
        [SerializeField] Color activeButtonColor;
        [SerializeField] Color activeButtonShadowColor;
        [SerializeField] Color disableButtonColor;
        [SerializeField] Color disableButtonShadowColor;
        [SerializeField] Sprite activeAdSprite;
        [SerializeField] Sprite disableAdSprite;

        [Space]
        [SerializeField] UIFade backgroundFade;

        [Space]
        [SerializeField] UIScalableObject levelCompleteLabel;

        [Space]
        [SerializeField] UIScalableObject moneyScalableObject;
        [SerializeField] Text moneyAmountText;

        [Space]
        [SerializeField] UIScalableObject rewardLabel;
        [SerializeField] Text rewardAmountsText;

        [Space]
        [SerializeField] UIScalableObject getX3ScalableObject;
        [SerializeField] Button getX3Button;
        [SerializeField] Image getX3ButtonImage;
        [SerializeField] Image adImage;

        [Space]
        [SerializeField] CanvasGroup continueButtonGroup;

        private TweenCase getX3PingPongCase;
        private Shadow getX3ButtonShadow;
        private bool IsAdAndRewardedVideoEnabled => (AdsManager.IsForcedAdEnabled() && AdsManager.IsRewardBasedVideoLoaded());
        private bool closePagePressed;

        public override void Init()
        {
            base.Init();

            getX3ButtonShadow = getX3ButtonImage.gameObject.GetComponent<Shadow>();
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

            rewardLabel.Hide(immediately: true);
            getX3ScalableObject.Hide(immediately: true);
            moneyScalableObject.Hide(immediately: true);

            //

            UpdateMoneyLabel();

            backgroundFade.Show(false, duration: 0.3f);
            levelCompleteLabel.Show(immediately: false);
            moneyScalableObject.Show(false, duration: 0.5f);

            UpdateGetX3Button();
            getX3ScalableObject.Show(false, duration: 0.5f, onCompleted: delegate
            {
                if (IsAdAndRewardedVideoEnabled)
                    getX3PingPongCase = getX3ScalableObject.RectTransform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);
            });

            ShowRewardLabel(LevelController.LevelCompleteReward, false, 0.3f, delegate
            {
                CurrencyCloudController.SpawnCurrency("Money".GetHashCode(), rewardAmountsText.rectTransform, moneyScalableObject.RectTransform, 15, "", () =>
                {
                    GameController.MoneyAmount += LevelController.LevelCompleteReward;
                    UpdateMoneyLabel();

                    rewardLabel.RectTransform.DOPushScale(Vector3.one * 1.1f, Vector3.one, 0.2f, 0.2f).OnComplete(delegate
                    {
                        ShowContinueButton(delayToShow: continueButtonAppearDelay, immediately: !IsAdAndRewardedVideoEnabled);
                    });
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


            levelCompleteLabel.Show(true);
            backgroundFade.Show(immediately: true);

            ShowRewardLabel(LevelController.LevelCompleteReward, true);
            GameController.MoneyAmount += LevelController.LevelCompleteReward;
            UpdateMoneyLabel();

            UpdateGetX3Button();
            getX3ScalableObject.Show(immediately: true);

            if (IsAdAndRewardedVideoEnabled)
                getX3PingPongCase = getX3ScalableObject.RectTransform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

            ShowContinueButton(delayToShow: continueButtonAppearDelay, immediately: !IsAdAndRewardedVideoEnabled);
        }

        public override void Hide()
        {
            if (!isPageDisplayed)
                return;

            float hideDuration = 0.4f;

            levelCompleteLabel.Hide(true, duration: hideDuration);
            backgroundFade.Hide(false, hideDuration);
            getX3ScalableObject.Hide(immediately: false, duration: hideDuration);

            rewardLabel.Hide(immediately: false, hideDuration);
            moneyScalableObject.Hide(immediately: false, duration: hideDuration);

            Tween.DelayedCall(hideDuration, delegate
            {

                canvas.enabled = false;
                isPageDisplayed = false;
            });
        }

        public override void HideImmediately()
        {
            if (!isPageDisplayed)
                return;

            canvas.enabled = false;
            isPageDisplayed = false;
        }


        #endregion

        private void UpdateMoneyLabel()
        {
            moneyAmountText.text = GameController.MoneyAmount.ToString();
        }

        #region RewardLabel

        public void ShowRewardLabel(float rewardAmounts, bool immediately = false, float duration = 0.3f, Action onComplted = null)
        {
            rewardLabel.Show(immediately);

            if (immediately)
            {
                rewardAmountsText.text = "+" + rewardAmounts;

                onComplted?.Invoke();
                return;
            }

            rewardAmountsText.text = "+" + 0;

            Tween.DoFloat(0, rewardAmounts, duration, (float value) =>
            {
                rewardAmountsText.text = "+" + (int)value;
            }).OnComplete(delegate
            {
                onComplted?.Invoke();
            });
        }

        #endregion

        #region Continue Lable

        public void ShowContinueButton(float delayToShow = 0.3f, bool immediately = true)
        {
            if (immediately)
            {
                continueButtonGroup.gameObject.SetActive(true);

                return;
            }

            Tween.DelayedCall(delayToShow, delegate
            {
                if (continueButtonGroup != null)
                    continueButtonGroup.gameObject.SetActive(true);
            });
        }

        public void HideContinueButton()
        {
            continueButtonGroup.gameObject.SetActive(false);
        }

        #endregion


        private void UpdateGetX3Button()
        {
            getX3Button.interactable = IsAdAndRewardedVideoEnabled;
            getX3ButtonImage.color = IsAdAndRewardedVideoEnabled ? activeButtonColor : disableButtonColor;
            getX3ButtonShadow.effectColor = IsAdAndRewardedVideoEnabled ? activeButtonShadowColor : disableButtonShadowColor;
            adImage.sprite = IsAdAndRewardedVideoEnabled ? activeAdSprite : disableAdSprite;
        }

        #region Buttons

        public void GetX3Button()
        {
            if (closePagePressed)
                return;

            closePagePressed = true;

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            AdsManager.ShowRewardBasedVideo((finished) =>
            {
                if (finished)
                {
                    ShowRewardLabel(LevelController.LevelCompleteReward * 3, onComplted: () =>
                    {
                        CurrencyCloudController.SpawnCurrency("Money".GetHashCode(), rewardAmountsText.rectTransform, moneyScalableObject.RectTransform, 15, "", () =>
                        {
                            GameController.MoneyAmount += LevelController.LevelCompleteReward * 3;
                            UpdateMoneyLabel();

                            rewardLabel.RectTransform.DOPushScale(Vector3.one * 1.1f, Vector3.one, 0.2f, 0.2f).OnComplete(delegate
                            {
                                GameController.OnCompletePageClosed();
                            });
                        });
                    });
                }
                else
                {
                    GameController.OnCompletePageClosed();
                }
            });
        }

        public void ContinueButton()
        {
            if (closePagePressed)
                return;

            closePagePressed = true;

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            GameController.OnCompletePageClosed();
        }

        #endregion
    }
}
