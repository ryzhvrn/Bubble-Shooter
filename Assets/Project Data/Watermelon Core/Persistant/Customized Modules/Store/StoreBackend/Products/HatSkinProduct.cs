using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class HatSkinProduct : StoreProduct
    {
        [SerializeField] Sprite unlockedIcon;
        public Sprite UnlockedIcon => unlockedIcon;

        [SerializeField] List<GameObject> prefabs = new List<GameObject>();
        public GameObject Prefab => prefabs.Count > 0 ? prefabs[Random.Range(0, prefabs.Count)] : null;

        [SerializeField] Vector3 hatOffset;
        public Vector3 HatOffset => hatOffset;

        [SerializeField] float storePreviewScaleMult;
        public float StorePreviewScaleMult => storePreviewScaleMult;


        public HatSkinProduct()
        {
            BehaviourType = BehaviourType.Default;
            Type = StoreProductType.HatSkin;
        }

        public override void Init()
        {
            base.Init();
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