using UnityEngine;

namespace Boardgame.Script {
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