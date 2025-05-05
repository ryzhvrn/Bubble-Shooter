using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Bezier Currency Animation", menuName = "Content/Currencies/Animations/Bezier")]
    public sealed class CurrencyAnimationBezier : CurrencyAnimation
    {
        [SerializeField] float delay = 0.18f;
        public float Delay => delay;

        [SerializeField] int amount = 2;
        public int Amount => amount;

        [Space]
        [SerializeField] Vector3 spawnOffset;
        public Vector3 SpawnOffset => spawnOffset;

        [SerializeField] DuoFloat durationRange;
        public DuoFloat DurationRange => durationRange;

        [SerializeField] DuoFloat scaleRange;
        public DuoFloat ScaleRange => scaleRange;

        [SerializeField] AnimationCurve scaleCurve;
        public AnimationCurve ScaleCurve => scaleCurve;

        protected override CurrencyAnimationCase CreateAnimationCase(Transform from, Transform to, Currency.Type currencyType, int amount, bool isFollowing)
        {
            return new CurrencyAnimationBezierCase(from, to, currencyType, amount, isFollowing, this);
        }
    }
}