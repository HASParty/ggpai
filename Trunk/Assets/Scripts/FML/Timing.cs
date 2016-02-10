using UnityEngine;
using System.Collections;

namespace Fml
{
    public enum Primitive
    {
        Immediately,
        MustEndBefore,
        ExecuteAnytimeDuring,
        StartImmediatelyAfter,
        StartSometimeAfter,
        StartTogether
    };

    public class Timing
    {

        public Primitive primitive { get; set; }
        public FMLChunk chunkReference { get; set; } // Reference to another FML chunk when required by the timing primitive chosen

        public Timing(Primitive prim, FMLChunk refChunk)
        {
            primitive = prim;
            chunkReference = refChunk;
        }
    }
}

