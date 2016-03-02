using UnityEngine;
using System.Collections;
using FML;

namespace Behaviour {
	public class Locomotion : BMLChunk {
		public override BMLChunkType Type { get { return BMLChunkType.Locomotion; } }
		public GameObject Target { get; private set; }
		public Lexemes.Locomotion Manner { get; private set; }

		//constructor
		public Locomotion(Participant character, GameObject target, Lexemes.Locomotion manner, 
		            float start = 0f, float end = 1f) 
		{
			Character = character;
			Target = target;
			Manner = manner;
			Start = start;
			End = end;
		}

		public override float GetTime(SyncPoints point) {
			switch (point) {
			case SyncPoints.Start:
				return Start;
			case SyncPoints.End:
				return Start + End;
			default:
				throw new System.ArgumentOutOfRangeException ();
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Locomotion: Start={0}, End={1}, Target={2}, Manner={3}]", Start, End, Target, Manner);
		}

	}
}
