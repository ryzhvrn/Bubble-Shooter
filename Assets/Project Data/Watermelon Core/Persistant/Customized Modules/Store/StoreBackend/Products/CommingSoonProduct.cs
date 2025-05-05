using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class CommingSoonProduct : StoreProduct
    {
        public CommingSoonProduct()
        {
            BehaviourType = BehaviourType.Dummy;
        }

        public override void Init()
        {

        }

        public override void Unlock()
        {

        }

        public override bool IsUnlocked()
        {
            return false;
        }

        public override bool CanBeUnlocked()
        {
            return false;
        }
    }
}