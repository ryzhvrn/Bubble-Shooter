using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    using Upgrades;

    [CreateAssetMenu(menuName = "Content/Upgrades/Player Speed Upgrade", fileName = "Player Speed Upgrade")]
    public class PlayerMovementSpeedUpgrade: Upgrade<PlayerMovementSpeedUpgrade.PlayerMovementSpeedUpgradeStage>
    {
        public override void Initialise()
        {

        }

        [System.Serializable]
        public class PlayerMovementSpeedUpgradeStage: BaseUpgradeStage
        {
            [SerializeField] float speed;
            public float Speed => speed;
        }
    }
}

