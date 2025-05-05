using UnityEngine;

namespace Watermelon.Upgrades
{
    [System.Serializable]
    public abstract class BaseUpgradeStage
    {
        [SerializeField] int price;
        public int Price => price;

        [SerializeField] Currency.Type currencyType;
        public Currency.Type CurrencyType => currencyType;

        [SerializeField] Sprite previewSprite;
        public Sprite PreviewSprite => previewSprite;

        [Header("Premium")]
        [SerializeField] bool isPremium;
        public bool IsPremium => isPremium;

        [SerializeField] Sprite premiumPreviewSprite;
        public Sprite PremiumPreviewSprite => premiumPreviewSprite;

        public void OverrideUpgradePrice(int value)
        {
            price = value;
        }
    }
}