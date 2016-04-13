using UnityEngine;
using System.Collections;

namespace Boardgame {
    /// <summary>
    /// Simple struct representing a cell
    /// </summary>
	[System.Serializable]
	public struct Cell {
		public string id;
		public float x;
		public float y;
		public float w;
		public float h;
	}
}