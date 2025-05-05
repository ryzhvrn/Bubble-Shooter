using UnityEngine;

namespace Watermelon
{
    public abstract class CurrencyAnimation : ScriptableObject
    {
        public virtual CurrencyAnimationCase Play(Transform from, Transform to, Currency.Type currencyType, int amount, bool isFollowing)
        {
            CurrencyAnimationCase currencyAnimationCase = CreateAnimationCase(from, to, currencyType, amount, isFollowing);

            return currencyAnimationCase;
        }

        protected abstract CurrencyAnimationCase CreateAnimationCase(Transform from, Transform to, Currency.Type currencyType, int amount, bool isFollowing);
    }
}