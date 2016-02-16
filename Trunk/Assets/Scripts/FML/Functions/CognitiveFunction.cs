using System.Collections.Generic;

namespace Fml
{
    public class CognitiveFunction : FMLFunction
    {
        public enum MentalState
        {
            PLANNING,
            THINKING,
            REMEMBERING
        }        

        public FunctionType Function { get { return FunctionType.COGNITIVE_PROCESS; } }
        public MentalState Type;

        public CognitiveFunction(MentalState type)
        {
            Type = type;
        }
    }
}
