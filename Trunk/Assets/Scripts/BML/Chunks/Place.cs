using UnityEngine;
using FML;

namespace Behaviour
{
    public class Place : BMLChunk
    {
        public override BMLChunkType Type { get { return BMLChunkType.Placing; } }
        /// <summary>
        /// Gets which hand the behaviour is to be executed on.
        /// </summary>
        /// <value>Which hand.</value>
        public Lexemes.Mode Mode { get; private set; }
        // <summary>
        /// Returns the target to be pointed at.
        /// </summary>
        /// <value>The target.</value>
        public GameObject Target { get; private set; }


        //sync points
        public float Ready { get; private set; }
        public float StrokeStart { get; private set; }
        public float Stroke { get; private set; }
        public float StrokeEnd { get; private set; }
        public float Relax { get; private set; }


        //constructor
        public Place(string id, Participant character, GameObject target, Lexemes.Mode mode,
                       float start = 0f, float ready = -1f, float strokeStart = -1f,
                       float stroke = -1f, float strokeEnd = -1f, float relax = -1f,
                       float end = 1f)
        {
            ID = id;
            Character = character;
            Target = target;
            Mode = mode;
            Start = start;
            StrokeStart = strokeStart;
            Stroke = stroke;
            StrokeEnd = strokeEnd;
            Ready = ready;
            Relax = relax;
            End = end;
        }

        public override float GetTime(SyncPoints point)
        {
            switch (point)
            {
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
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            return string.Format("[Placing: Start={0}, End={1}, Mode={2}, Target={3}, Ready={4}, StrokeStart={5}, Stroke={6}, StrokeEnd={7}, Relax={8}]", Start, End, Mode, Target, Ready, StrokeStart, Stroke, StrokeEnd, Relax);
        }
    }
}
