using System.Collections;
using UnityEngine;

namespace Watermelon
{
    public abstract class CurrencyAnimationCase
    {
        protected Transform fromTransform;
        protected Transform toTrasnform;

        protected Currency currency;
        protected int amount;

        protected bool isPlaying;
        protected bool isFollowing;

        protected Coroutine coroutine;

        // Events
        public OnItemMovementCompletedCallback OnItemMovementCompleted;
        public OnAnimationCompletedCallback OnAnimationCompleted;
        
        public CurrencyAnimationCase(Transform from, Transform to, Currency.Type currencyType, int amount, bool isFollowing = false)
        {
            this.amount = amount;
            this.isFollowing = isFollowing;

            fromTransform = from;
            toTrasnform = to;

            currency = CurrenciesController.GetCurrency(currencyType);
        }

        public void Play()
        {
            if (isPlaying)
                return;

            isPlaying = true;

            coroutine = CurrenciesController.PlayCoroutine(AnimationCoroutine());
        }

        public virtual void Stop()
        {
            if (!isPlaying)
                return;

            isPlaying = false;
        }

        protected abstract IEnumerator AnimationCoroutine();

        public delegate void OnItemMovementCompletedCallback();
        public delegate void OnAnimationCompletedCallback();
    }
}