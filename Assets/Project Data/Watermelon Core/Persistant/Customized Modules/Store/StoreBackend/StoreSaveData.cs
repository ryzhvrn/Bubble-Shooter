using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon
{
    [System.Serializable]
    public class StoreSaveData
    {
        public List<int> unlockedProductsIds = new List<int>();

        public int selectedCharacterSkinId;
        public int selectedEnvironmentSkinId;

        public StoreSaveData()
        {
            unlockedProductsIds = new List<int>();
        }

        public bool IsProductUnlocked(int id)
        {
            return unlockedProductsIds.FindIndex(x => x == id) != -1;
        }

        public void UnlockProduct(int id)
        {
            if (unlockedProductsIds.FindIndex(x => x == id) == -1)
                unlockedProductsIds.Add(id);
        }

#if UNITY_EDITOR
        public void ClearUnlockedProductsData()
        {
            unlockedProductsIds.Clear();
        }

        public void RemoveProductFormUnlocked(int id)
        {
            int itemIndex = unlockedProductsIds.FindIndex(x => x == id);

            if (itemIndex != -1)
                unlockedProductsIds.RemoveAt(itemIndex);
        }
#endif
    }
}