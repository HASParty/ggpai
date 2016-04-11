using UnityEngine;
using System.Collections;

namespace Boardgame.Script {
    [System.Serializable]
    public struct PhysicalPieces {
        public string Type;
        public GameObject Prefab;
    }

    [System.Serializable]
    public struct PiecePosition {
        public string cellID;
        public string pieceType;
    }

    public class BoardgameScriptable : ScriptableObject {
        public string ID;
        public GridScriptable PhysicalBoardDescription;
        public GameObject PhysicalBoardPrefab;
        public PhysicalPieces[] PhysicalPieces;
        public string SecondPile;
        public string FirstPile;
        public string TrashPile;
    }
}