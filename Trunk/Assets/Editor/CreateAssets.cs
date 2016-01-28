using UnityEngine;
using UnityEditor;

static class CreateAssets {
	
	[MenuItem("Assets/Create/Scriptables/Boardgame Grid")]
	public static void CreateGridScriptable() {
		ScriptableObjectUtility.CreateAsset<Boardgame.Script.GridScriptable>();
	}

    [MenuItem("Assets/Create/Scriptables/Boardgame physical definition")]
    public static void CreateBoardgameScriptable()
    {
        ScriptableObjectUtility.CreateAsset<Boardgame.Script.BoardgameScriptable>();
    }

}