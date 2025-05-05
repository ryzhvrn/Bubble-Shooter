using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class GunSkinProduct : StoreProduct
    {
        [SerializeField] Sprite preview;
        public Sprite Preview => preview;

        [SerializeField] GunData gunData;
        public GunData GunData => gunData;

        [SerializeField] float storePreviewScaleMult;
        public float StorePreviewScaleMult => storePreviewScaleMult;

        [SerializeField] float storePreviewRotationY;
        public float StorePreviewRotationY => storePreviewRotationY;

        public GunSkinProduct()
        {
            BehaviourType = BehaviourType.Default;
            Type = StoreProductType.GunSkin;
        }

        public override void Init()
        {
            base.Init();
            GunData.Init();
        }

        public override void Unlock()
        {
            GameController.MoneyAmount -= StoreController.Database.GetProductPrice(Type);
        }

        public override bool IsUnlocked()
        {
            return StoreController.IsProductUnlocked(ID);
        }

        public override bool CanBeUnlocked()
        {
            return GameController.MoneyAmount >= StoreController.Database.GetProductPrice(Type);
        }
    }
}