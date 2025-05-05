using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class ProductPreviewController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] float hideDuration = 0.5f;

        [Space]
        [SerializeField] StoreViewMode viewMode;

        [Space]
        [SerializeField] Transform previewHolderTransform;
        [SerializeField] Camera previewCameraRenderer;

        [Header("Visualizer")]
        [ShowIf("Is3DProductPreview")]
        [SerializeField] ProductVisualizer3D productVisualizer3D;

        [Header("Visualizer")]
        [ShowIf("Is2DProductPreview")]
        [SerializeField] ProductVisualizer2D productVisualizer2D;

        private ProductVisualizer generalVisualizer;

        private void OnEnable()
        {
            StoreController.OnProductSelected += VisualizeProduct;
        }

        private void OnDisable()
        {
            StoreController.OnProductSelected -= VisualizeProduct;
        }

        public void Init()
        {
            // initializer preview object and disable unused one
            if (viewMode == StoreViewMode.View3D)
            {
                if (productVisualizer2D != null && productVisualizer2D.gameObject.activeSelf)
                    productVisualizer2D.gameObject.SetActive(false);

                if (productVisualizer3D != null && !productVisualizer3D.gameObject.activeSelf)
                    productVisualizer3D.gameObject.SetActive(true);

                generalVisualizer = productVisualizer3D;
            }
            else
            {
                if (productVisualizer3D != null && productVisualizer3D.gameObject.activeSelf)
                    productVisualizer3D.gameObject.SetActive(false);

                if (productVisualizer2D != null && !productVisualizer2D.gameObject.activeSelf)
                    productVisualizer2D.gameObject.SetActive(true);

                generalVisualizer = productVisualizer2D;
            }

            // visualize all selected products
            //List<StoreProductType> types = StoreController.GetAllProductTypes();

            //for (int i = 0; i < types.Count; i++)
            //{
            //    VisualizeProduct(StoreController.GetSelectedProduct(types[i]));
            //}
        }

        public void ShowPreview()
        {
            previewHolderTransform.gameObject.SetActive(true);
            previewHolderTransform.transform.localScale = Vector3.one;
            previewCameraRenderer.enabled = true;
        }

        public void HidePreview(bool immediately = false)
        {
            if (immediately)
            {
                previewCameraRenderer.enabled = false;
                previewHolderTransform.gameObject.SetActive(false);
            }
            else
            {
                previewHolderTransform.transform.DOPushScale(Vector3.one * 1.05f, Vector3.zero, hideDuration * 0.36f, hideDuration * 0.64f, Ease.Type.SineOut, Ease.Type.SineIn).OnComplete(delegate
                {
                    previewCameraRenderer.enabled = false;
                    previewHolderTransform.gameObject.SetActive(false);
                });
            }
        }

        public void VisualizeProduct(StoreProduct product)
        {
            generalVisualizer.Visualize(product);
        }

        #region Editor Conditionals
        private bool Is3DProductPreview()
        {
            return viewMode == StoreViewMode.View3D;
        }

        private bool Is2DProductPreview()
        {
            return viewMode == StoreViewMode.View2D;
        }
        #endregion
    }

    public enum StoreViewMode
    {
        View2D = 0,
        View3D = 1,
    }
}
