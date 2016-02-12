using Fml;

namespace Behaviour {
	/// <summary>
	/// Temporarily change the actor's facial expression.
	/// </summary>
	public class Face : BmlChunk {
		public override BmlChunkType Type { get { return BmlChunkType.Face; } }
		/// <summary>
		/// Gets the facial expression to be shown.
		/// </summary>
		/// <value>The facial expression lexeme.</value>
		public Lexemes.Face Lexeme { get; private set; }
		/// <summary>
		/// Returs the time of the peak of the expression.
		/// </summary>
		/// <value>float in seconds</value>
		public float AttackPeak { get; private set; }
		/// <summary>
		/// Returns when the facial animation starts to revert.
		/// </summary>
		/// <value>float in seconds</value>
		public float Relax { get; private set; }

		/// <summary>
		/// Gets the modifier.
		/// </summary>
		/// <value>The modifier of the intensity of the expression (0-1 where 0 is normal and 1 is exaggerated).</value>
		public float Modifier { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="VirtualReykjavik.Behaviour.Face"/> class.
		/// </summary>
		/// <param name="character">Character to execute the behaviour.</param>
		/// <param name="lexeme">Which expression to execute.</param>
		/// <param name="start">Start of animation from the given moment.</param>
		/// <param name="attackPeak">Attack peak - peak of animation.</param>
		/// <param name="relax">Relax - when expression starts ending.</param>
		/// <param name="end">End - expression ended.</param>
		public Face(string id, Participant character, Lexemes.Face lexeme, 
		            float start, float attackPeak, float relax, float end, float modifier = 0) 
		{
			ID = id;
			Character = character;
			Lexeme = lexeme;
			Start = start;
			AttackPeak = attackPeak;
			Relax = relax;
			End = end;
			Modifier = modifier;
		}

		public override float GetTime(SyncPoints point) {
			switch (point) {
			case SyncPoints.Start:
				return Start;
			case SyncPoints.Relax:
				return Start + Relax;
			case SyncPoints.End:
				return Start + End;
			case SyncPoints.AttackPeak:
				return Start + AttackPeak;
			default:
				throw new System.ArgumentOutOfRangeException ();
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Face: Start={0}, End={1}, Lexeme={2}, AttackPeak={3}, Relax={4}]", Start, End, Lexeme, AttackPeak, Relax);
		}
	}
}
