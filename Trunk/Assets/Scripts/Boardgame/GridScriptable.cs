using UnityEngine;

namespace Boardgame.Script
{
    //Simply describes the positions of cells on a physical mesh.
    //For visual interactive editing create a new GridScriptable, attach
    //a Grid script to a textured game board mesh, and set the Grid
    //source to the new scriptable, and enable editing.
    public class GridScriptable : ScriptableObject
    {
        public Cell[] grid;
    }
}
