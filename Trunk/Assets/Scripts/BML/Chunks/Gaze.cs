using UnityEngine;
using System.Collections;
using Fml;

namespace Behaviour {
	/// <summary>
	/// Temporarily have the actor gaze upon another target.
	/// </summary>
	public class Gaze : BMLChunk {
		public override BMLChunkType Type { get { return BMLChunkType.Gaze; } }
		/// <summary>
		/// Returns the target to be gazed at.
		/// </summary>
		/// <value>The target.</value>
		public GameObject Target { get; private set; }
		/// <summary>
		/// Returns which part of the spine/head/eyes is to be influenced to execute the behaviour.
		/// </summary>
		/// <value>The part influenced.</value>
		public Lexemes.Influence Influence { get; private set; }

		/// <summary>
		/// The time when the actor is successfully gazing at the target.
		/// </summary>
		/// <value>float in seconds relative to start</value>
		public float Ready { get; private set; }
		/// <summary>
		/// The time when the actor starts looking away from the target.
		/// </summary>
		/// <value>float in seconds relative to start</value>
		public float Relax { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="VirtualReykjavik.Behaviour.Gaze"/> class.
		/// </summary>
		/// <param name="id">Identifier for the behaviour.</param>
		/// <param name="character">Character to execute the behaviour.</param>
		/// <param name="target">Target to be gazed at.</param>
		/// <param name="influence">Body part to be influenced by the headlook controller.</param>
		/// <param name="start">When the character starts to gaze.</param>
		/// <param name="ready">When the character is currently gazing.</param>
		/// <param name="relax">When the character starts looking away.</param>
		/// <param name="end">When the character is done looking.</param>
		public Gaze(string id, Participant character, GameObject target, Lexemes.Influence influence, 
		            float start = 0f, float ready = -1f, float relax = -1f, float end = 1f) 
		{
			ID = id;
			Character = character;
			Target = target;
			Influence = influence;
			Start = start;
			Ready = ready;
			Relax = relax;
			End = end;
		}

		public override float GetTime(SyncPoints point) {
			switch (point) {
			case SyncPoints.Start:
				return Start;
			case SyncPoints.Relax:
				return Start + Relax;
			case SyncPoints.End:
				return Start + End;
			case SyncPoints.Ready:
				return Start + Ready;
			default:
				throw new System.ArgumentOutOfRangeException ();
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Gaze: Start={0}, End={1}, Target={2}, Influence={3}, Ready={4}, Relax={5}]", Start, End, Target, Influence, Ready, Relax);
		}

	}
}
