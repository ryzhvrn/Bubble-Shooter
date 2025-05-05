using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public static class TimeController
    {
        private const float DEFAULT_TIME = 1.0f;

        private static TweenCase timeTweenCase;

        public static void LerpTime(AnimationCurve timeCurve, float time)
        {
            if (timeTweenCase != null && !timeTweenCase.isCompleted)
                timeTweenCase.Kill();

            timeTweenCase = Tween.DoFloat(0.0f, DEFAULT_TIME, time, (value) =>
            {
                SetTime(timeCurve.Evaluate(value));
            }, 0, true).OnComplete(delegate
            {
                SetTime(DEFAULT_TIME);
            });
        }

        public static void ResetSmooth(float time, Ease.Type easing)
        {
            if (Time.timeScale != DEFAULT_TIME)
            {
                if (timeTweenCase != null && !timeTweenCase.isCompleted)
                    timeTweenCase.Kill();

                timeTweenCase = Tween.DoFloat(Time.timeScale, DEFAULT_TIME, time, (value) =>
                {
                    SetTime(value);
                }, 0, true).SetEasing(easing);
            }
        }

        public static void ResetTime()
        {
            SetTime(DEFAULT_TIME);
        }

        private static void SetTime(float value)
        {
            Time.timeScale = value;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
    }
}