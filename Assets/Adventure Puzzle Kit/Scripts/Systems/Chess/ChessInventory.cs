using System.Collections.Generic;
using UnityEngine;

namespace AdventurePuzzleKit.ChessSystem
{
    public class ChessInventory : MonoBehaviour
    {
        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true;

        [Space(5)]
        public List<ChessPiece> chessPieceList = new List<ChessPiece>();

        public static ChessInventory instance;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        public void AddChessPiece(ChessPiece chessPiece)
        {
            if (!chessPieceList.Contains(chessPiece))
            {
                chessPieceList.Add(chessPiece);
                AKUIManager.instance.FillChessInventorySlot();
                AKUIManager.instance.ChessPieceCollected();
            }
        }

        public void RemoveChessPiece(ChessPiece chessPiece)
        {
            if (chessPieceList.Contains(chessPiece))
            {
                int currentCount = chessPieceList.Count;
                chessPieceList.Remove(chessPiece);
                AKUIManager.instance.ResetChessInventorySlot();
            }
        }
    }
}
