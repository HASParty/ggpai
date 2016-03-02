using System.Collections.Generic;

namespace FML {
    public class EmotionFunction : FMLFunction {

        public FunctionType Function { get { return FunctionType.EMOTION; } }

        public float Arousal;
        public float Valence;

        public EmotionFunction(float arousal, float valence) {
            Arousal = arousal;
            Valence = valence;
        }
    }
}
