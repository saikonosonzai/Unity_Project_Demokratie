/// REMEMBER: This script has a custom editor called "LeverItemEditor", found in the "Editor" folder. You will need to add new properties to this
/// if you create new variables / fields in this script. Contact me if you have any troubles at all!

using UnityEngine;

namespace AdventurePuzzleKit.LeverSystem
{
    public class LeverItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemType _itemType = ItemType.None;
        public enum ItemType { None, Lever, TestButton, ResetButton }

        [SerializeField] private int leverNumber = 1;
        [SerializeField] private string animationName = "Handle_Pull";

        [SerializeField] private LeverSystemController _leverSystemController = null;

        private Animator handleAnimation;

        private void Start()
        {
            handleAnimation = GetComponentInChildren<Animator>();
        }

        public void StartLooking() { } //Started looking at the Padlock object

        public void StopInteraction() { } //Stopped interacting with the Padlock object

        public void HandleInputClick()
        {
            //Started interaction with the Padlock object

            switch (_itemType)
            {
                case ItemType.Lever:
                    LeverNumber();
                    break;
                case ItemType.TestButton:
                    LeverCheck();
                    break;
                case ItemType.ResetButton:
                    LeverReset();
                    break;
            }
        }

        public void HandleInputHold() { }//Holding interaction with the Padlock object

        public void HandleInputStop() { } //Stopped interaction with the Padlock object

        public void LeverNumber()
        {
            _leverSystemController.InitLeverPull(this, leverNumber);
        }

        public void HandleAnimation()
        {
            handleAnimation.Play(animationName, 0, 0.0f);
        }

        public void LeverReset()
        {
            _leverSystemController.LeverReset();
        }

        public void LeverCheck()
        {
            _leverSystemController.LeverCheck();
        }
    }
}
