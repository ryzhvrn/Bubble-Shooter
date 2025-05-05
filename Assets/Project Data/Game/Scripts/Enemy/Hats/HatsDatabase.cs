using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BubbleShooter
{
    [CreateAssetMenu(fileName = "Hats Database", menuName = "Content/Enemies/Hats Database")]
    public class HatsDatabase : ScriptableObject
    {
        [SerializeField] HatCase[] hats;

        private static HatCase[] activeHats;
        private static Dictionary<HatType, HatCase> hatsLink;

        public void Init()
        {
            hatsLink = new Dictionary<HatType, HatCase>();
            activeHats = hats;

            for (int i = 0; i < hats.Length; i++)
            {
                hatsLink.Add(hats[i].HatType, hats[i]);
            }
        }

        public static HatCase GetHat(HatType hatType)
        {
            if (hatType == HatType.None)
                return null;

            if (hatType == HatType.Random)
                return activeHats.GetRandomItem();

#if DEBUG_LOGS
            if (!hatsLink.ContainsKey(hatType))
                Debug.LogError($"[Hats Database]: Hat with type {hatType} is missing!");
#endif

            return hatsLink[hatType];
        }
    }
}