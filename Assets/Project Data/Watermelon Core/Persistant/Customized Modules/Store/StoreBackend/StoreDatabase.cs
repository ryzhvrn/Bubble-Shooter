using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [SetupTab("Store Database", texture = "icon_cart")]
    [CreateAssetMenu(fileName = "Store Database", menuName = "Content/Store Database")]
    public class StoreDatabase : ScriptableObject, IInitialized
    {
        [SerializeField] StoreProduct[] products;
        public StoreProduct[] Products => products;

        [SerializeField] int gunSkinPrice;
        [SerializeField] int hatSkinPrice;
        [SerializeField] int coinsForAdsAmount;

        public List<StoreProductType> allTypesList { get; private set; }
        public int CoinsForAdsAmount => coinsForAdsAmount;

        public int GetProductPrice(StoreProductType type)
        {
            if (type == StoreProductType.GunSkin)
                return gunSkinPrice;
            else
                return hatSkinPrice;
        }

        public void Init()
        {
            allTypesList = new List<StoreProductType>();

            for (int i = 0; i < products.Length; i++)
            {
                products[i].Init();

                if(!allTypesList.Contains(products[i].Type))
                {
                    allTypesList.Add(products[i].Type);
                }
            }
        }

        public List<int> GetDefaultSkinsIDs()
        {
            List<int> defaultSkins = new List<int>();

            for (int i = 0; i < Products.Length; i++)
            {
                if (products[i].IsDefault || products[i].BehaviourType == BehaviourType.Random)
                {
                    defaultSkins.Add(products[i].ID);
                }
            }

            if (defaultSkins.Count <= 0)
            {
                Debug.LogError("[Store Module]: No default skins was found. You probably need to add at least one. Default skins will be unlocked and selected on the first launch.");
            }

            return defaultSkins;
        }

        public List<StoreProduct> GetProductsByPage(StorePageName page, StoreProductType productType)
        {
            List<StoreProduct> groupSkins = new List<StoreProduct>();

            foreach (StoreProduct product in products)
            {
                if (product.Type == productType && product.Page == page)
                {
                    groupSkins.Add(product);
                }
            }

            return groupSkins;
        }

        public List<StoreProduct> GetAllProductsByPage(StorePageName page)
        {
            List<StoreProduct> groupSkins = new List<StoreProduct>();

            foreach (StoreProduct product in products)
            {
                if (product.Page == page)
                {
                    groupSkins.Add(product);
                }
            }

            return groupSkins;
        }

        public Dictionary<StorePageName, List<StoreProduct>> GetProductsByPageDictionary(StoreProductType productType)
        {
            Dictionary<StorePageName, List<StoreProduct>> result = new Dictionary<StorePageName, List<StoreProduct>>();

            foreach (StorePageName page in Enum.GetValues(typeof(StorePageName)))
            {
                result.Add(page, GetProductsByPage(page, productType));
            }

            return result;
        }

        public List<int> GetPagesAmountPerProducts()
        {
            List<int> result = new List<int>();

            int productsTypesAmount = Enum.GetValues(typeof(StoreProductType)).Length;
            int maxPagesAmount = System.Enum.GetNames(typeof(StorePageName)).Length;


            for (int i = 0; i < productsTypesAmount; i++)
            {
                result.Add(0);
            }

            for (int page = 0; page < maxPagesAmount; page++)
            {
                List<StoreProduct> products = GetAllProductsByPage((StorePageName)page);

                for (int i = 0; i < productsTypesAmount; i++)
                {
                    if (products.FindIndex(p => p.Type == (StoreProductType)i) != -1)
                    {
                        result[i]++;
                    }
                }
            }

            return result;
        }

        public StoreProduct GetRandomLockedProduct()
        {
            List<StoreProduct> lockedProductsList = new List<StoreProduct>();

            for (int i = 0; i < products.Length; i++)
            {
                if (products[i].BehaviourType != BehaviourType.Dummy && !products[i].IsUnlocked())
                {
                    lockedProductsList.Add(products[i]);
                }
            }

            if (lockedProductsList.Count > 0)
            {
                return lockedProductsList[UnityEngine.Random.Range(0, lockedProductsList.Count)];
            }
            else
            {
                return null;
            }
        }

        public StoreProduct GetRandomUnlockedProduct(StoreProductType type)
        {
            List<StoreProduct> unlockedProductList = new List<StoreProduct>();

            for (int i = 0; i < products.Length; i++)
            {
                if (products[i].Type == type && products[i].BehaviourType != BehaviourType.Dummy && products[i].BehaviourType != BehaviourType.Random && products[i].IsUnlocked())
                {
                    unlockedProductList.Add(products[i]);
                }
            }

            if (unlockedProductList.Count > 0)
            {
                return unlockedProductList[UnityEngine.Random.Range(0, unlockedProductList.Count)];
            }
            else
            {
                return null;
            }
        }
    }
}