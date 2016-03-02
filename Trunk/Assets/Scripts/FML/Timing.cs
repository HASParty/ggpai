using UnityEngine;
using System.Collections;

namespace FML
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

        public Primitive Primitive { get; set; }
        public FMLChunk ChunkReference { get; set; } // Reference to another FML chunk when required by the timing primitive chosen

        public Timing(Primitive prim, FMLChunk refChunk)
        {
            Primitive = prim;
            ChunkReference = refChunk;
        }
    }
}

