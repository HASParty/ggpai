using System.Collections.Generic;

namespace Fml
{
    public class EmotionFunction : FMLFunction
    {
        public enum EmotionalState
        {
            ANGER,
            DIGUST,
            EMBARRASSMENT,
            FEAR,
            HAPPINESS,
            SADNESS,
            SURPRISE,
            SHAME,
            NEUTRAL
        }

        public enum EmotionRegulation
        {
            FELT,
            FAKED,
            LEAKED
        }

        public FunctionType Function { get { return FunctionType.EMOTION; } }

        public EmotionalState Type;
        public EmotionRegulation Regulation;
        public float Intensity;
        public float WeightFactor;

        public EmotionFunction(EmotionalState type, EmotionRegulation regulation, float intensity, float weightFactor)
        {
            Type = type;
            Regulation = regulation;
            Intensity = intensity;
            WeightFactor = weightFactor;
        }
    }
}
