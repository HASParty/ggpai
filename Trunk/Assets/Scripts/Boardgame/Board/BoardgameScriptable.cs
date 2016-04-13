using UnityEngine;
using System.Collections;

namespace Boardgame.Script {
    /// <summary>
    /// Describes physical pieces as defined by type designations in the
    /// GDL and what asset to use to represent the piece
    /// </summary>
    [System.Serializable]
    public struct PhysicalPieces {
        public string Type;
        public GameObject Prefab;
    }

    /// <summary>
    /// Where a piece should be initialised at
    /// and of what type
    /// </summary>
    [System.Serializable]
    public struct PiecePosition {
        public string cellID;
        public string pieceType;
    }

    /// <summary>
    /// Scriptable that represents
    /// the pieces and description of the board
    /// (helps with mapping the GDL to actual in-game things)
    /// </summary>
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