using System.Collections;
using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public class FoamBehaviour : MonoBehaviour
    {
        private const float SCALE_TIME = 0.5f;

        [SerializeField] GameObject foamPrefab;

        [Header("Settings")]
        [SerializeField] float radius;
        [SerializeField] int elementsAmount;


        private FoamCase[] foamCases;

        private Pool foamPartsPool;
        private Coroutine animationCoroutine;

        private void Awake()
        {
            foamPartsPool = new Pool(new PoolSettings("Foam", foamPrefab, elementsAmount, false, transform));
        }

        public void Spawn(float size)
        {
            foamPartsPool.ReturnToPoolEverything();

            foamCases = new FoamCase[elementsAmount];
            for (int i = 0; i < elementsAmount; i++)
            {
                GameObject foamObject = foamPartsPool.GetPooledObject();
                foamObject.transform.localScale = Vector3.zero;
                foamObject.transform.localPosition = Vector3.zero;
                foamObject.transform.localRotation = Random.rotation;

                foamObject.SetActive(true);

                foamCases[i] = new FoamCase(foamObject.transform, Random.insideUnitSphere * radius, size, Random.Range(0.4f, 1.0f));
            }

            animationCoroutine = StartCoroutine(AnimationCoroutine());
        }

        public void Resize(float multiplier)
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            for (int i = 0; i < elementsAmount; i++)
            {
                foamCases[i].MultiplyScale(multiplier);
            }

            animationCoroutine = StartCoroutine(AnimationScaleCoroutine());
        }

        private IEnumerator AnimationCoroutine()
        {
            float state = 0.0f;
            for (state = 0; state < 1.0f; state += Time.deltaTime / SCALE_TIME)
            {
                for (int i = 0; i < elementsAmount; i++)
                {
                    foamCases[i].Invoke(state);
                }

                yield return null;
            }
        }

        private IEnumerator AnimationScaleCoroutine()
        {
            float state = 0.0f;
            for (state = 0; state < 1.0f; state += Time.deltaTime / SCALE_TIME)
            {
                for (int i = 0; i < elementsAmount; i++)
                {
                    foamCases[i].InvokeScale(state);
                }

                yield return null;
            }
        }

        private class FoamCase
        {
            private Transform targetTransform;
            private Vector3 targetPosition;
            private Vector3 targetScale;
            private float targetTime;

            private Vector3 startLocalPosition;
            private Vector3 startLocalScale;

            public FoamCase(Transform targetTransform, Vector3 targetPosition, float targetScale, float targetTime)
            {
                this.targetTransform = targetTransform;
                this.targetPosition = targetPosition;
                this.targetScale = targetScale.ToVector3();
                this.targetTime = targetTime;

                startLocalPosition = targetTransform.localPosition;
                startLocalScale = targetTransform.localScale;
            }

            public void MultiplyScale(float multiplier)
            {
                startLocalScale = targetScale;

                targetScale = targetScale * multiplier;
            }

            public void Invoke(float state)
            {
                targetTransform.localPosition = Vector3.Lerp(startLocalPosition, targetPosition, state);
                targetTransform.localScale = Vector3.Lerp(startLocalScale, targetScale, Mathf.InverseLerp(0, targetTime, state));
            }

            public void InvokeScale(float state)
            {
                targetTransform.localScale = Vector3.Lerp(startLocalScale, targetScale, Mathf.InverseLerp(0, targetTime, state));
            }
        }
    }
}
