using UnityEngine;
using System.Collections;

namespace Boardgame {
	[System.Serializable]
	public struct Cell {
		public string id;
		public float x;
		public float y;
		public float w;
		public float h;
		public Piece piece;
	}
}