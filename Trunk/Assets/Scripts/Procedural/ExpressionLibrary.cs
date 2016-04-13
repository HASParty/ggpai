using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Parses expressions.txt and loads up the expressions defined in there
/// and makes them available for look up in the face controller
/// </summary>
public class ExpressionLibrary {

	private static Dictionary<string, Dictionary<string, Vector3>> lib;
	private static Regex reg = new Regex(@"<.+>[^<]+");
	
	static void Populate() {
        lib = new Dictionary<string, Dictionary<string, Vector3>>();
		TextAsset ta = Resources.Load<TextAsset>("expressions");
		MatchCollection matches = reg.Matches (ta.text);
		//Debug.Log (matches.Count);
		foreach(Match m in matches) {
			string[] split = m.Value.Split ('\n');
			string name = split[0].Replace ("<","").Replace (">","").Trim ();
			lib.Add (name, new Dictionary<string, Vector3>());
			for(int i = 1; i < split.Length-1; i++) {
				var a = split[i].Split (',');
				//Debug.Log (a[0]);
				Vector3 vec = new Vector3(float.Parse (a[1]), float.Parse(a[2]), float.Parse(a[3].Trim ()));
				//Debug.Log (a[0]);
				lib[name].Add (a[0], vec);
			}
		}
	}
	
	public static bool Contains(string expression) {
		if(lib == null) Populate();
		return lib.ContainsKey(expression);
	}
	
	public static Vector3 Get(string expression, string bone) {
		if(lib == null) Populate(); 
		if(lib[expression].ContainsKey(bone)) {
			return lib[expression][bone];
		} else {
			return Vector3.zero;
		}
	}
	
	public static Dictionary<string, Vector3> Get(string expression) {
		if(lib == null) Populate();
		if(lib.ContainsKey(expression)) {
			return lib[expression];
		} else {
			Debug.Log ("Invalid expression");
			return new Dictionary<string, Vector3>();
		}
	}

}
