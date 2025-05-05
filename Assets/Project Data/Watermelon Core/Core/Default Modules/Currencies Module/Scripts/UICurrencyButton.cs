using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UICurrencyButton : MonoBehaviour
    {
        [SerializeField] Currency.Type currency;
        [SerializeField] DisableMode disableMode;

        [HideIf("IsColorMode")]
        [SerializeField] Sprite activeButtonSprite;
        [HideIf("IsColorMode")]
        [SerializeField] Sprite disabledButtonSprite;

        [HideIf("IsSpriteMode")]
        [SerializeField] Color activeButtonColor;
        [HideIf("IsSpriteMode")]
        [SerializeField] Color disabledButtonColor;

        [Space]
        [SerializeField] Button buttonRef;
        [SerializeField] Image buttonImage;
        [SerializeField] Text buttonText;
        [SerializeField] Image currencyImage;
        [SerializeField] CanvasGroup textAndIconCanvasGroup;

        private int currentPrice;

        private void OnEnable()
        {
            CurrenciesController.OnCurrencyAmountChanged += OnCurrencyChanged;
        }

        public void Init(int price)
        {
            Init(price, currency);
        }

        public void Init(int price, Currency.Type currencyType)
        {
            currency = currencyType;
            currentPrice = price;

            currencyImage.sprite = CurrenciesController.GetCurrency(currencyType).Icon;
            buttonText.text = CurrenciesHelper.Format(price);

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // activate button
            if (CurrenciesController.Get(currency) >= currentPrice)
            {
                buttonRef.interactable = true;
                textAndIconCanvasGroup.alpha = 1f;

                if (disableMode == DisableMode.Sprite)
                {
                    buttonImage.sprite = activeButtonSprite;
                }
                else
                {
                    buttonImage.color = activeButtonColor;
                }
            }
            // disable button
            else
            {
                buttonRef.interactable = false;
                textAndIconCanvasGroup.alpha = 0.6f;

                if (disableMode == DisableMode.Sprite)
                {
                    buttonImage.sprite = disabledButtonSprite;
                }
                else
                {
                    buttonImage.color = disabledButtonColor;
                }
            }
        }

        private void OnCurrencyChanged(Currency.Type currencyType, int amount, int amountDifference)
        {
            if (currencyType == currency)
            {
                UpdateVisuals();
            }
        }

        private void OnDisable()
        {
            CurrenciesController.OnCurrencyAmountChanged -= OnCurrencyChanged;
        }

        #region Editor

        private enum DisableMode
        {
            Sprite = 0,
            Color = 1,
        }

        private bool IsColorMode()
        {
            return disableMode == DisableMode.Color;
        }

        private bool IsSpriteMode()
        {
            return disableMode == DisableMode.Sprite;
        }

        #endregion
    }
}