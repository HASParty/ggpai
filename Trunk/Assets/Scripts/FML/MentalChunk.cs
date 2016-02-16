using System.Collections.Generic;

namespace Fml
{
    public class MentalChunk : FMLChunk
    {
        public TrackType TrackType { get { return TrackType.MentalState; } }

        public bool AddFunction(FMLFunction func)
        {
            if (func.Function == FMLFunction.FunctionType.COGNITIVE_PROCESS || 
                func.Function == FMLFunction.FunctionType.EMOTION)
            {
                functions.Add(func);
                return true;
            }
            return false;
        }

        public MentalChunk()
        {
            functions = new List<FMLFunction>();
        }
    }
}
