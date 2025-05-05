using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BubbleShooter;

namespace Watermelon
{
    public class StorePage : MonoBehaviour
    {
        [SerializeField] GridLayoutGroup gridLayourGroup;

        private List<StoreItemUI> storeItemsList = new List<StoreItemUI>();
        public List<StoreItemUI> StoreItemsList => storeItemsList;

        private Pool storeItemPool;

        public void Init(List<StoreProduct> products)
        {
            UIStore uiStore = UIController.GetPage<UIStore>();

            storeItemPool = PoolManager.GetPoolByName(uiStore.STORE_ITEM_POOL_NAME);
            storeItemsList.Clear();

            gridLayourGroup.enabled = true;
            int selectedProductId = StoreController.GetSelectedProductSkinID(UIStore.CurrentProductType);

            for (int i = 0; i < products.Count; i++)
            {
                StoreItemUI item = storeItemPool.GetPooledObject(new PooledObjectSettings().SetParrent(transform)).GetComponent<StoreItemUI>();
                storeItemsList.Add(item);

                item.transform.localScale = Vector3.one;
                item.transform.SetParent(transform);

                item.Init(products[i], products[i].ID == selectedProductId);
            }

            Tween.DelayedCall(0.1f, () => gridLayourGroup.enabled = false);
        }

        public void UpdatePage()
        {
            int selectedProductId = StoreController.GetSelectedProductSkinID(UIStore.CurrentProductType);

            for (int i = 0; i < storeItemsList.Count; i++)
            {
                storeItemsList[i].UpdateItem(storeItemsList[i].ProductRef.ID == selectedProductId);
            }
        }
    }
}