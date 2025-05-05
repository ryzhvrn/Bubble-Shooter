using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BubbleShooter;

namespace Watermelon
{
    public class StoreItemUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] Color outlineColorNeutral;
        [SerializeField] Color outlineColorSelected;

        [SerializeField] Color backColorNeutral;
        [SerializeField] Color backShadowColorNeutral;

        [SerializeField] Color backColorSelected;
        [SerializeField] Color backShadowColorSelected;

        [Header("References")]
        [SerializeField] Image productImage;
        [SerializeField] Image outlineImage;
        [SerializeField] Image lockedImage;
        [SerializeField] Image backImage;

        public StoreProduct ProductRef { get; private set; }

        private Shadow backShadow;

        private void Awake()
        {
            backShadow = backImage.GetComponent<Shadow>();
        }

        public void Init(StoreProduct product, bool isSelected)
        {
            ProductRef = product;

            UpdateItem(isSelected);
        }

        public void UpdateItem(bool isSelected)
        {
            if (ProductRef == null)
            {
                Debug.LogError("Store Items is not initialized.");
                return;
            }



            SetHighlightState(isSelected);

            if (ProductRef.Type == StoreProductType.GunSkin)
            {
                productImage.sprite = ((GunSkinProduct)ProductRef).Preview;
            }
            else
            {
                productImage.sprite = ((HatSkinProduct)ProductRef).UnlockedIcon;
            }

            lockedImage.enabled = !ProductRef.IsUnlocked();
        }

        public void OnClick()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (ProductRef.IsUnlocked())
            {
                StoreController.TryToSelectProduct(ProductRef.ID);
            }
        }

        public void SetHighlightState(bool active)
        {
            outlineImage.color = active ? outlineColorSelected : outlineColorNeutral;
            backImage.color = active ? backColorSelected : backColorNeutral;
            backShadow.effectColor = active ? backShadowColorSelected : backShadowColorNeutral;
        }
    }
}