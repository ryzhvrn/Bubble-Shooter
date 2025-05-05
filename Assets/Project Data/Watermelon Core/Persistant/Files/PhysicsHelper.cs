using UnityEngine;

namespace Watermelon
{
    public static class PhysicsHelper
    {
        public static readonly int LAYER_INTERACTABLE = LayerMask.NameToLayer("Interactable");
        public static readonly int LAYER_ENVIRONMENT = LayerMask.NameToLayer("Environment");
        public static readonly int LAYER_FLOOR = LayerMask.NameToLayer("Floor");
        public static readonly int LAYER_ENEMY = LayerMask.NameToLayer("Enemy");
        public static readonly int LAYER_DEFAULT = LayerMask.NameToLayer("Default");

        public const string TAG_ENEMY = "Enemy";
        public const string TAG_GUN = "Gun";
        public const string TAG_PLAYER = "Player";

        public static void Init()
        {

        }
    }
}