using UnityEngine;
using UnityEditor;

static class CreateAssets {
	
	[MenuItem("Assets/Create/Scriptables/Boardgame Grid")]
	public static void CreateGridScriptable() {
		ScriptableObjectUtility.CreateAsset<Boardgame.GridScriptable>();
	}
	
}