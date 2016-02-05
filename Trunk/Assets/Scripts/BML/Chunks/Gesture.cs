using UnityEngine;
using System.Collections;

namespace Behaviour {
	public class Gesture : BmlChunk {
		public override BmlChunkType Type { get { return BmlChunkType.Gesture; } }
		/// <summary>
		/// Gets which hand the behaviour is to be executed on.
		/// </summary>
		/// <value>Which hand.</value>
		public Lexemes.Mode Mode { get; private set; }
		/// <summary>
		/// Gets which gesture is to be executed.
		/// </summary>
		/// <value>The gesture lexeme.</value>
		public Lexemes.Gestures Lexeme { get; private set; }

		//sync points
		public float Ready { get; private set; }
		public float StrokeStart { get; private set; }
		public float Stroke { get; private set; }
		public float StrokeEnd { get; private set; }
		public float Relax { get; private set; }

		//constructor
		public Gesture(string id, Participant character, Lexemes.Mode mode, Lexemes.Gestures lexeme, 
		               float start = 0f, float ready = -1f, float strokeStart = -1f, 
		               float stroke = -1f, float strokeEnd = -1f, float relax = -1f, 
		               float end = 1f) 
		{
			ID = id;
			Character = character;
			Mode = mode;
			Lexeme = lexeme;
			Start = start;
			StrokeStart = strokeStart;
			Stroke = stroke;
			StrokeEnd = strokeEnd;
			Ready = ready;
			Relax = relax;
			End = end;
		}


		public override float GetTime(SyncPoints point) {
			switch (point) {
			case SyncPoints.Start:
				return Start;
			case SyncPoints.Ready:
				return Start + Ready;
			case SyncPoints.Relax:
				return Start + Relax;
			case SyncPoints.End:
				return Start + End;
			case SyncPoints.StrokeStart:
				return Start + StrokeStart;
			case SyncPoints.Stroke:
				return Start + Stroke;
			case SyncPoints.StrokeEnd:
				return Start + StrokeEnd;
			default:
				throw new System.ArgumentOutOfRangeException ();
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Gesture: Start={0}, End={1}, Mode={2}, Lexeme={3}, Ready={4}, StrokeStart={5}, Stroke={6}, StrokeEnd={7}, Relax={8}]", Start, End, Mode, Lexeme, Ready, StrokeStart, Stroke, StrokeEnd, Relax);
		}
	}
}
