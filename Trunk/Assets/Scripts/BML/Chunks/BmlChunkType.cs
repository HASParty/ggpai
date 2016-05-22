using UnityEngine;
using System.Collections;

namespace Behaviour {
    /// <summary>
    /// Types of BML chunks
    /// </summary>
	public enum BMLChunkType {
        Face,
        Gaze,
        GazeShift,
        Gesture,
        Head,
        Locomotion,
        Posture,
        Speech,
        Pointing,
        //Boardgame additions
        FaceEmotion,
        Grasping,
        Placing,
        Vocalisation
    }

}
