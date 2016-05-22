using UnityEngine;

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
}
