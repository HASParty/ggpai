using UnityEngine;
using System.Collections;
using FML;

namespace Behaviour {
    /// <summary>
    /// A gesture the actor should execute
    /// </summary>
	public class Gesture : BMLChunk {
		public override BMLChunkType Type { get { return BMLChunkType.Gesture; } }
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Gesture"/> class.
        /// </summary>
        /// <param name="id">the name of the chunk</param>
        /// <param name="character">the actor</param>
        /// <param name="mode">which hand, etc.</param>
        /// <param name="lexeme">what gesture</param>
        /// <param name="start">time of the start of the gesture</param>
        /// <param name="ready">the time when the gesture is ready to stroke</param>
        /// <param name="strokeStart">when the stroke begins</param>
        /// <param name="stroke">the beat of the stroke</param>
        /// <param name="strokeEnd">the end of the stroke</param>
        /// <param name="relax">when the gesture begins to relax</param>
        /// <param name="end">the end of the gesture</param>
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
