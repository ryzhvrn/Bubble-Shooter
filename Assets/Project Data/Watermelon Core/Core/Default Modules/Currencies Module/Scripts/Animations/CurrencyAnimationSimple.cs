using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Simple Currency Animation", menuName = "Content/Currencies/Animations/Simple")]
    public sealed class CurrencyAnimationSimple : CurrencyAnimation
    {
        [SerializeField] float duration = 1.0f;
        public float Duration => duration;

        [SerializeField] float delay = 0.18f;
        public float Delay => delay;

        [Space]
        [SerializeField] Ease.Type movementEasing;
        public Ease.Type MovementEasing => movementEasing;

        [SerializeField] AnimationCurve movementCurve;
        public AnimationCurve MovementCurve => movementCurve;

        [SerializeField] AnimationCurve scaleCurve;
        public AnimationCurve ScaleCurve => scaleCurve;

        protected override CurrencyAnimationCase CreateAnimationCase(Transform from, Transform to, Currency.Type currencyType, int amount, bool isFollowing)
        {
            return new CurrencyAnimationSimpleCase(from, to, currencyType, amount, isFollowing, this);
        }
    }
}