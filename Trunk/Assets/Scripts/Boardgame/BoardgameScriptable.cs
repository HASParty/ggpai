using UnityEngine;
using System.Collections;

namespace Boardgame.Script {
    [System.Serializable]
    public struct PhysicalPieces
    {
        public string Type;
        public Player Player;
        public GameObject Prefab;
    }
    [System.Serializable]
    public struct PiecePosition
    {
        public bool notOnBoard;
        public int count;
        public string cellID;
        public string pieceType;
    }

    public class BoardgameScriptable : ScriptableObject
    {
        public string ID;
        public GridScriptable PhysicalBoardDescription;
        public GameObject PhysicalBoardPrefab;
        public PhysicalPieces[] PhysicalPieces;
        //temporary until this can be generated from the GDL
        public PiecePosition[] InitialWhitePieces;
        public PiecePosition[] InitialBlackPieces;
    }
}