using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.BubbleShooter;

namespace Watermelon
{
    [System.Serializable]
    public class ProductVisualizer3D : ProductVisualizer
    {
        [SerializeField] Transform objectHolder;

        private GameObject previewObject;

        public override void Visualize(StoreProduct product)
        {
            if (product.Type == StoreProductType.GunSkin)
            {
                GunSkinProduct prod = (GunSkinProduct)product;
                SpawnPreview(prod.GunData.GunPrefab, prod.StorePreviewScaleMult,  new Vector3(-0.4f,-0.9f,0f), prod.StorePreviewRotationY);
            }
            if (product.Type == StoreProductType.HatSkin)
            {
                HatSkinProduct prod = (HatSkinProduct)product;
                SpawnPreview(prod.Prefab, prod.StorePreviewScaleMult, Vector3.zero);
            }
        }

        public void SpawnPreview(GameObject prefab, float scaleMult, Vector3 positionOffset, float rotationY = 0)
        {
            ClearPreview();

            previewObject = Instantiate(prefab);

            previewObject.transform.SetParent(objectHolder);

            previewObject.transform.localScale = Vector3.one * scaleMult;
            previewObject.transform.localPosition = Vector3.zero + positionOffset;
            previewObject.transform.localRotation = Quaternion.Euler(0f, rotationY, 0f);
        }

        private void ClearPreview()
        {
            if (previewObject == null)
                return;

            Destroy(previewObject);
        }
    }
}
