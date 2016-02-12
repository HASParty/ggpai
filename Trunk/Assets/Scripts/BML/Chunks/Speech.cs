using UnityEngine;
using System.Collections;
using Fml;

namespace Behaviour {
	public class Speech : BmlChunk {
		public override BmlChunkType Type { get { return BmlChunkType.Speech; } }

		public string Content {
			get;
			private set;
		}

		public bool Generate {
			get;
			private set;
		}

		public Speech(string id, Participant character, float start, float end, string content, bool speak = true) {
			ID = id;
			Character = character;
			Start = start;
			End = end;
			Content = content;
			Generate = speak;
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
			return string.Format ("[Speech: Start={0}, End={1}, Content={2}]", Start, End, Content);
		}
	}
}
