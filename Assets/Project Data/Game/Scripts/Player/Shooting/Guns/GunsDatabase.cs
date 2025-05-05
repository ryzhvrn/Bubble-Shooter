using UnityEngine;
using Watermelon;

namespace Watermelon.BubbleShooter
{
    [CreateAssetMenu(fileName = "Guns Database", menuName = "Content/Guns/Guns Database")]
    public class GunsDatabase : ScriptableObject
    {
        [SerializeField] GunData[] guns;
        public GunData[] Guns => guns;

        private GameObject bulletsContainerObject;

        public void Init()
        {
            // Create bullets container object
            bulletsContainerObject = new GameObject("[BULLETS CONTAINER]");

            // Reset position, rotation and scale
            bulletsContainerObject.transform.ResetLocal();

            // Mark as non destroyable
            DontDestroyOnLoad(bulletsContainerObject);

            // Initialise guns
            for (int i = 0; i < guns.Length; i++)
            {
                guns[i].Init();
            }
        }
    }
}