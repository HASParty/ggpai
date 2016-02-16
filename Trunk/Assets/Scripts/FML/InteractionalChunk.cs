using System.Collections.Generic;

namespace Fml
{
    public class InteractionalChunk : FMLChunk
    {
        public TrackType TrackType { get { return TrackType.Interactional; } }

        public bool AddFunction(FMLFunction func)
        {
            //maybe give functions track types...?
            if(func.Function == FMLFunction.FunctionType.GROUNDING  ||
               func.Function == FMLFunction.FunctionType.CLOSING    ||
               func.Function == FMLFunction.FunctionType.INITIATION ||
               func.Function == FMLFunction.FunctionType.SPEECH_ACT ||
               func.Function == FMLFunction.FunctionType.TURN_TAKING)
            {
                functions.Add(func);
                return true;
            }
            return false;
        }

        public InteractionalChunk()
        {
            functions = new List<FMLFunction>();
        }
    }
}
