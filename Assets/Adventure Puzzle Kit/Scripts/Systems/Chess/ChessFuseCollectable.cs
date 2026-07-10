using UnityEngine;

namespace AdventurePuzzleKit.ChessSystem
{
    public class ChessFuseCollectable : MonoBehaviour
    {
        [Header("Chess Piece ScriptableObject")]
        [SerializeField] private ChessPiece chessPieceScriptable = null;

        [Header("Pickup Audio Clip")]
        [SerializeField] private Sound pickupSound = null;

        public void PickupChessPiece()
        {
            ChessInventory.instance.AddChessPiece(chessPieceScriptable);

            AKAudioManager.instance.Play(pickupSound);
            gameObject.SetActive(false);
        }
    }
}
