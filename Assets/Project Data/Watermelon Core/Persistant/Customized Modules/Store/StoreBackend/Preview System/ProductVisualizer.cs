using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public abstract class ProductVisualizer : MonoBehaviour
    {
        public abstract void Visualize(StoreProduct product);
    }
}