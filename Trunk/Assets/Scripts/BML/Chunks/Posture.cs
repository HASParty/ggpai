using FML;
using System.Collections.Generic;

namespace Behaviour {
	public class Pose { 
		public Lexemes.BodyPart Part { get; private set; }
		public Lexemes.BodyPose Lexeme { get; private set; }
        public int Degree { get; private set; }

		public Pose(Lexemes.BodyPart part, Lexemes.BodyPose lexeme, int degree) {
			Part = part;
			Lexeme = lexeme;
            Degree = degree;
		}

		public override string ToString ()
		{
			return string.Format ("[{0}, {1}, {2}]", Part, Lexeme, Degree);
		}
	}

	public class Posture : BMLChunk {
		public override BMLChunkType Type { get { return BMLChunkType.Posture; } }
		public Lexemes.Stance Stance { get; private set; }
		public List<Pose> Poses { get; private set; }

		public Posture(string id, Participant character, Lexemes.Stance stance = Lexemes.Stance.STANDING, 
		            float start = 0f, float end = 1f, int priority = 1) 
		{
			ID = id;
			Character = character;
			Stance = stance;
			Start = start;
			End = end;
            Poses = new List<Pose>();
            Priority = priority;
		}

		public void AddPose(Lexemes.BodyPart part, Lexemes.BodyPose lexeme, int degree = 0) {
			Poses.Add (new Pose(part, lexeme, degree));
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
			return string.Format ("[Posture: Start={0}, End={1}, Stance={2}, Poses={3}]", Start, End, Stance, Poses);
		}
	}
}
