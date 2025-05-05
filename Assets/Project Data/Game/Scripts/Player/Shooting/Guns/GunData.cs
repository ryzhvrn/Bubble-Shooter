using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    public abstract class GunData : ScriptableObject
    {
        [SerializeField] GameObject gunPrefab;
        public GameObject GunPrefab => gunPrefab;

        [SerializeField] GameObject bulletPrefab;
        public GameObject BulletPrefab => bulletPrefab;

        [SerializeField] GameObject bulletSplashPrefab;
        public GameObject BulletSplashPrefab => bulletSplashPrefab;

        [SerializeField] float bulletSpeed;
        public float BulletSpeed => bulletSpeed;

        [SerializeField] int damage;
        public int Damage => damage;

        private Pool bulletsPool;
        public Pool BulletsPool => bulletsPool;

        private Pool bulletSplashPool;
        public Pool BulletSplashPool => bulletSplashPool;

        public void Init()
        {
            bulletsPool = PoolManager.GetPoolByName(bulletPrefab.name);
            bulletSplashPool = PoolManager.GetPoolByName(bulletSplashPrefab.name);


            if(bulletsPool == null)
            {
                bulletsPool = PoolManager.AddPool(new PoolSettings(bulletPrefab.name, bulletPrefab, 3, true));
            }

            if(bulletSplashPool == null)
            {
                bulletSplashPool = PoolManager.AddPool(new PoolSettings(bulletSplashPrefab.name, bulletSplashPrefab, 1, true));
            }
        }

        public void ResetBullets()
        {
#if UNITY_EDITOR
            if (bulletsPool == null)
            {
                Debug.LogError($"Gun {GetType()} isn't initialised!");

                return;
            }
#endif

            bulletsPool.ReturnToPoolEverything(true);
            bulletSplashPool.ReturnToPoolEverything(true);
        }
    }
}