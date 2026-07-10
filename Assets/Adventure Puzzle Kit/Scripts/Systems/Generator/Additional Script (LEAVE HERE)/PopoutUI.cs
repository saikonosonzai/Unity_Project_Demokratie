using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AdventurePuzzleKit.GeneratorSystem
{
    [System.Serializable]
    public class PopoutUI
    {
        [Header("Item Parameters")]
        public string itemName = null;
        public Sprite iconImage = null;

        [Header("UI - World Space Floating Canvas")]
        public GameObject itemCanvas = null;

        [Header("UI - World Space Floating Elements")]
        public TMP_Text itemNameUI = null;
        public Image iconImageUI = null;
        public TMP_Text fuelAmountUI = null;
        public TMP_Text maxFuelAmountUI = null;
        public Image fuelGaugeUI = null;
    }
}
