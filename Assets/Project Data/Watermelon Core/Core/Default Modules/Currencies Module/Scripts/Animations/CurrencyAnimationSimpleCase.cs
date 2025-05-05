using System.Collections;
using UnityEngine;

namespace Watermelon
{
    public sealed class CurrencyAnimationSimpleCase : CurrencyAnimationCase
    {
        private CurrencyAnimationSimple animationSettings;

        public CurrencyAnimationSimpleCase(Transform from, Transform to, Currency.Type currencyType, int amount, bool isContinues, CurrencyAnimationSimple animationSettings) : base(from, to, currencyType, amount, isContinues)
        {
            this.animationSettings = animationSettings;

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
            
            Pool pool = currency.Pool;

            GameObject[] spawnedObjects = new GameObject[amount];
            float[] spawnedObjectsStates = new float[amount];
            bool[] objectsState = new bool[amount];

            float spawnTime = float.MinValue;

            int spawnedAmount = 0;
            int disabledAmount = 0;

            bool isUpdateActive = true;

            while (isUpdateActive)
            {
                if (isPlaying && Time.time > spawnTime && spawnedAmount < amount)
                {
                    GameObject spawnedGameObject = pool.GetPooledObject();
                    spawnedGameObject.transform.position = fromTransform.position;
                    spawnedGameObject.transform.rotation = Quaternion.identity;
                    spawnedGameObject.transform.localScale = Vector3.one;
                    spawnedGameObject.SetActive(true);

                    spawnedObjects[spawnedAmount] = spawnedGameObject;
                    objectsState[spawnedAmount] = true;

                    spawnedAmount++;

                    spawnTime = Time.time + animationSettings.Delay;
                }

                for (int i = disabledAmount; i < spawnedAmount; i++)
                {
                    if (objectsState[i])
                    {
                        spawnedObjectsStates[i] += Time.deltaTime / animationSettings.Duration;

                        Vector3 lerpedPosition = Vector3.Lerp(fromTransform.position, toTrasnform.position, Ease.Interpolate(spawnedObjectsStates[i], animationSettings.MovementEasing));
                        lerpedPosition.y = Mathf.Lerp(fromTransform.position.y, toTrasnform.position.y, spawnedObjectsStates[i]) + animationSettings.MovementCurve.Evaluate(spawnedObjectsStates[i]);

                        spawnedObjects[i].transform.position = lerpedPosition;
                        spawnedObjects[i].transform.localScale = Vector3.one * animationSettings.ScaleCurve.Evaluate(spawnedObjectsStates[i]);

                        if (spawnedObjectsStates[i] >= 1.0f)
                        {
                            objectsState[i] = false;
                            spawnedObjects[i].SetActive(false);

                            disabledAmount++;

                            OnItemMovementCompleted?.Invoke();

                            if (disabledAmount == spawnedAmount)
                                isUpdateActive = false;
                        }
                    }
                }

                yield return null;
            }

            OnAnimationCompleted?.Invoke();
        }
    }
}