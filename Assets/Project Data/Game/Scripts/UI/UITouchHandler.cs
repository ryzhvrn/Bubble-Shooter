#pragma warning disable 0414

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Watermelon.BubbleShooter;

namespace Watermelon
{

    // UI Module v0.9.0
    public class UITouchHandler : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            GameController.OnTapToShoot();
        }

    }
}