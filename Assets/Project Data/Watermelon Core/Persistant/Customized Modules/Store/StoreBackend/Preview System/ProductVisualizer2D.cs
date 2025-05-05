using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ProductVisualizer2D : ProductVisualizer
    {
        [SerializeField] SpriteRenderer spriteRenderer;

        public override void Visualize(StoreProduct product)
        {
            Debug.Log("[Store module] Implement visualization of the product here");
        }
    }
}
