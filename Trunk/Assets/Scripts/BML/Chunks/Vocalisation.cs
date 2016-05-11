using FML;

namespace Behaviour {
	public class Vocalisation : BMLChunk {
		public override BMLChunkType Type { get { return BMLChunkType.Vocalisation; } }

        public float Arousal { get; private set; }
        public float Valence { get; private set; }

		public Vocalisation(string id, Participant character, float start, float arousal, float valence) {
			ID = id;
			Character = character;
			Start = start;
			End = 1f;
            Arousal = arousal;
            Valence = valence;
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
			return string.Format ("[Vocalisation: Start={0}, Arousal={1} Valence={2}]", Start, Arousal, Valence);
		}
	}
}
