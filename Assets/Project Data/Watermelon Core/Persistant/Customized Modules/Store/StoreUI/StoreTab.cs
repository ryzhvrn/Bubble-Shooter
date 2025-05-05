using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BubbleShooter;

namespace Watermelon
{
    public class StoreTab : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] StoreProductType type;
        [SerializeField] string tabName;
        public StoreProductType Type => type;

        [Space(5f)]
        [SerializeField] Color frontSelectedColor = Color.white;
        [SerializeField] Color frontNeutralColor = Color.white;

        [SerializeField] Color backSelectedColor = Color.white;
        [SerializeField] Color backNeutralColor = Color.white;


        [Header("References")]
        [SerializeField] Image frontPanelImage;
        [SerializeField] Shadow shadow;
        [SerializeField] Text tabText;

        private void Start()
        {
            tabText.text = tabName;
        }

        public void SetActiveState(bool active)
        {
            frontPanelImage.color = active ? frontSelectedColor : frontNeutralColor;
            shadow.effectColor = active ? backSelectedColor : backNeutralColor;
        }

        public void OnClick()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            UIController.GetPage<UIStore>().OnTabPressed(type);         
        }
    }
}