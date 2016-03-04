using FML;

namespace Behaviour {
    /// <summary>
    /// Shift face into expressing current emotion
    /// </summary>
    public class FaceEmotion : BMLChunk {
        public override BMLChunkType Type { get { return BMLChunkType.FaceEmotion; } }

        public float Arousal;
        public float Valence;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualReykjavik.Behaviour.Face"/> class.
        /// </summary>
        /// <param name="character">Character to execute the behaviour.</param>
        /// <param name="lexeme">Which expression to execute.</param>
        /// <param name="start">Start of animation from the given moment.</param>
        public FaceEmotion(string id, Participant character, Lexemes.Face lexeme,
                    float start, float arousal, float valence) {
            ID = id;
            Character = character;
            Start = start;
            End = -1f;
            Arousal = arousal;
            Valence = valence;
        }

        public override float GetTime(SyncPoints point) {
            switch (point) {
                case SyncPoints.Start:
                    return Start;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        public override string ToString() {
            return string.Format("[Face: Start={0}, Arousal={1}, Valence={2}]", Start, Arousal, Valence);
        }
    }
}
