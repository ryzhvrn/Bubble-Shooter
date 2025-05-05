using System.Collections;
using UnityEngine;

namespace Watermelon
{
    public sealed class CurrencyAnimationBezierCase : CurrencyAnimationCase
    {
        private CurrencyAnimationBezier animationSettings;

        private Ease.IEasingFunction sineInOut;
        private Ease.IEasingFunction quadInOut;

        public CurrencyAnimationBezierCase(Transform from, Transform to, Currency.Type currencyType, int amount, bool isFollowing, CurrencyAnimationBezier animationSettings) : base(from, to, currencyType, amount, isFollowing)
        {
            this.animationSettings = animationSettings;

            sineInOut = Ease.GetFunction(Ease.Type.SineInOut);
            quadInOut = Ease.GetFunction(Ease.Type.QuadOutIn);

            Play();
        }

        public override void Stop()
        {
            if (!isPlaying)
                return;

            isPlaying = false;
        }

        protected override IEnumerator AnimationCoroutine()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(animationSettings.Delay);

            // Calculate total particles amount
            int totalParticlesAmount = amount * animationSettings.Amount;

            Pool pool = currency.Pool;

            ParticleInfo[] particles = new ParticleInfo[totalParticlesAmount];
            float spawnTime = float.MinValue;

            int spawnedAmount = 0;
            int disabledAmount = 0;

            bool isUpdateActive = true;

            while (isUpdateActive)
            {
                // Create new particle
                if (isPlaying && Time.time > spawnTime && spawnedAmount < totalParticlesAmount)
                {
                    for(int i = 0; i < animationSettings.Amount; i++)
                    {
                        var startPos = fromTransform.position + fromTransform.rotation * animationSettings.SpawnOffset;

                        var direction = toTrasnform.position - startPos;

                        var rotation = Quaternion.FromToRotation(Vector3.forward, direction);

                        var right = rotation * Vector3.right;

                        var key1 = startPos + Vector3.up * Random.Range(3f, 6f) + right * Random.Range(-6f, 6f) + direction.normalized * Random.Range(-2f, 2f);
                        var key2 = toTrasnform.position + Vector3.up * Random.Range(3f, 6f) + right * Random.Range(-6f, 6f) + direction.normalized * Random.Range(-2f, 2f);

                        GameObject spawnedGameObject = pool.GetPooledObject();
                        spawnedGameObject.transform.position = startPos;
                        spawnedGameObject.transform.rotation = Quaternion.Euler(Random.Range(-30f, 30f), Random.Range(-30f, 30f), Random.Range(-30f, 30f));
                        spawnedGameObject.transform.localScale = Vector3.zero;
                        spawnedGameObject.SetActive(true);

                        particles[spawnedAmount] = new ParticleInfo()
                        {
                            particle = spawnedGameObject.transform,

                            startPoint = startPos,
                            keyPoint1 = key1,
                            keyPoint2 = key2,

                            endPoint = toTrasnform.position,

                            scale = Vector3.one * animationSettings.ScaleRange.Random(),

                            time = 0,
                            duration = animationSettings.DurationRange.Random()
                        };

                        spawnedAmount++;
                    }

                    spawnTime = Time.time + animationSettings.Delay;
                }

                for (int i = disabledAmount; i < spawnedAmount; i++)
                {
                    if(isFollowing)
                    {
                        particles[i].endPoint = toTrasnform.position;
                    }

                    particles[i].time += Time.deltaTime / particles[i].duration;

                    particles[i].particle.position = Bezier.EvaluateCubic(particles[i].startPoint, particles[i].keyPoint1, particles[i].keyPoint2, particles[i].endPoint, sineInOut.Interpolate(Mathf.Clamp01(particles[i].time)));
                    particles[i].particle.localScale = particles[i].scale * animationSettings.ScaleCurve.Evaluate(particles[i].time);

                    if (particles[disabledAmount].time >= 1.0f)
                    {
                        // Disable particle 
                        particles[disabledAmount].particle.gameObject.SetActive(false);

                        if(disabledAmount % animationSettings.Amount == 0)
                            OnItemMovementCompleted?.Invoke();

                        disabledAmount++;

                        if (disabledAmount == totalParticlesAmount)
                            isUpdateActive = false;
                    }
                }

                yield return null;
            }

            OnAnimationCompleted?.Invoke();
        }

        private class ParticleInfo
        {
            public Transform particle;

            public Vector3 startPoint;
            public Vector3 endPoint;
            public Vector3 keyPoint1;
            public Vector3 keyPoint2;

            public Vector3 scale;

            public float time;
            public float duration;
        }
    }
}