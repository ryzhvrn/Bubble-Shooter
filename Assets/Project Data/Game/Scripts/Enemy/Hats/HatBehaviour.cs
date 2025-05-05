using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    [System.Serializable]
    public class HatBehaviour
    {
        [SerializeField] Transform holderTransform;
        public Transform HolderTrasform => holderTransform;

        [SerializeField] HatType defaultHatType = HatType.Random;

        private GameObject hatObject;
        private Rigidbody hatRigidbody;

        private bool isInitialised;

        public void Init(HatSkinProduct product)
        {
            if (hatObject != null)
                DestroyCurrentHat();

            if (defaultHatType != HatType.None && product != null)
            {
                hatObject = Object.Instantiate(product.Prefab);
                hatObject.transform.SetParent(holderTransform);
                hatObject.transform.ResetLocal();
                hatObject.transform.localPosition = product.HatOffset;

                hatRigidbody = hatObject.GetComponent<Rigidbody>();
                hatRigidbody.isKinematic = true;

                isInitialised = true;
            }
        }

        private void DestroyCurrentHat()
        {
            Object.Destroy(hatObject);
            hatObject = null;
            isInitialised = false;
        }

        public void DetachHat()
        {
            if (!isInitialised)
                return;

            hatObject.transform.SetParent(null);

            hatRigidbody.isKinematic = false;

            hatRigidbody.AddRelativeForce(new Vector3(Random.Range(-0.15f, 0.15f), 0.77f, -0.3f) * 50f, ForceMode.Impulse);
            hatRigidbody.AddRelativeTorque(hatObject.transform.right * Random.Range(-50f, 50f), ForceMode.Impulse);

            Tween.DelayedCall(1.5f, delegate
            {
                hatObject.transform.DOScale(0.0f, 0.4f).SetEasing(Ease.Type.CircIn).OnComplete(delegate
                {
                    hatObject.SetActive(false);
                });
            });
        }

        public void InitHatHolderTransform(Transform holder)
        {
            holderTransform = holder;
        }
    }
}