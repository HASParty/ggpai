using UnityEngine;
using System.Collections;
using Fml;

namespace Behaviour {
	public class BmlChunk {
		public string ID { get; set; }
		/// <summary>
		/// Return the character that is to execute the behaviour chunk.
		/// </summary>
		/// <value>The character.</value>
		public Participant Character { get; protected set; }
		/// <summary>
		/// Get the type of the chunk.
		/// </summary>
		/// <value>The type of the chunk.</value>
		public virtual BmlChunkType Type { get; protected set; }

		/// <summary>
		/// Returns the start sync point of the behaviour.
		/// </summary>
		/// <value>A float in seconds.</value>
		public float Start { get; protected set; }
		/// <summary>
		/// Returns the end sync point of the behaviour.
		/// </summary>
		/// <value>A float in seconds (relative to start).</value>
		public float End { get; protected set; }

		public virtual float GetTime(SyncPoints point) {
			return 0f;
		}

		public void Sync(SyncPoints thisPoint, float withThis) {
			float point = GetTime (thisPoint);
			float diff = point - withThis;
			Debug.Log(ID + "point " + point + " diff " + diff);
			Start -= diff;
		}

	}
}
