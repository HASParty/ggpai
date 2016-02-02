using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class ExpressionLibrary {

	private static Dictionary<string, Dictionary<string, Vector3>> lib;
	private static Regex reg = new Regex(@"<.+>[^<]+");
	
	static void Populate() {
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
		if(lib == null) initialise();
		return lib.ContainsKey(expression);
	}
	
	public static Vector3 Get(string expression, string bone) {
		if(lib == null) initialise(); 
		if(lib[expression].ContainsKey(bone)) {
			return lib[expression][bone];
		} else {
			return Vector3.zero;
		}
	}
	
	public static Dictionary<string, Vector3> Get(string expression) {
		if(lib == null) initialise();
		if(lib.ContainsKey(expression)) {
			return lib[expression];
		} else {
			Debug.Log ("Invalid expression");
			return new Dictionary<string, Vector3>();
		}
	}
	
	//temporary
	private static void initialise() {
		var shapeTENSE = new Dictionary<string, Vector3>();
		shapeTENSE.Add ("LipLowerR",new Vector3(-0.00147541f, 0f, -0.0009836066f));
		shapeTENSE.Add ("LipLowerL",new Vector3(-0.00147541f, 0f, -0.0009836066f));
		shapeTENSE.Add ("Jaw",new Vector3(0f, 0f, 0f));
		shapeTENSE.Add ("LipCornerL",new Vector3(0f, 0f, 0f));
		shapeTENSE.Add ("LipCornerR",new Vector3(0f, 0f, 0f));
		shapeTENSE.Add ("LipUpperL",new Vector3(0f, 0f, 0f));
		shapeTENSE.Add ("LipUpperR",new Vector3(0f, 0f, 0f));
		
		var shapeRELAXED = new Dictionary<string, Vector3>();
		shapeRELAXED.Add ("LipLowerR",new Vector3(0.00295082f, 0f, 0f));
		shapeRELAXED.Add ("LipLowerL",new Vector3(0.00295082f, 0f, 0f));
		shapeRELAXED.Add ("Jaw",new Vector3(0.001967213f, 0f, 0f));
		shapeRELAXED.Add ("LipCornerL",new Vector3(0f, 0.0002918032f, 0f));
		shapeRELAXED.Add ("LipCornerR",new Vector3(0f, -0.0002918032f, 0f));
		shapeRELAXED.Add ("LipUpperL",new Vector3(0f, 0f, 0f));
		shapeRELAXED.Add ("LipUpperR",new Vector3(0f, 0f, 0f));
		
		var shapeAW = new Dictionary<string, Vector3>();
		shapeAW.Add ("LipLowerR",new Vector3(0.002459016f, 0f, 0f));
		shapeAW.Add ("LipLowerL",new Vector3(0.002459016f, 0f, 0f));
		shapeAW.Add ("Jaw",new Vector3(0.002459016f, 0f, 0f));
		shapeAW.Add ("LipCornerL",new Vector3(0.0009836066f, -0.003934426f, 0f));
		shapeAW.Add ("LipCornerR",new Vector3(0.0009836066f, 0.003934426f, 0f));
		shapeAW.Add ("LipUpperL",new Vector3(0f, 0f, 0f));
		shapeAW.Add ("LipUpperR",new Vector3(0f, 0f, 0f));
		
		var shapeAH = new Dictionary<string, Vector3>();
		shapeAH.Add ("LipLowerR",new Vector3(0.001967213f, 0f, 0f));
		shapeAH.Add ("LipLowerL",new Vector3(0.001967213f, 0f, 0f));
		shapeAH.Add ("Jaw",new Vector3(0.003442623f, 0f, 0f));
		shapeAH.Add ("LipCornerL",new Vector3(0.0009836066f, 0.0004918032f, 0f));
		shapeAH.Add ("LipCornerR",new Vector3(0.0009836066f, -0.0004918032f, 0f));
		shapeAH.Add ("LipUpperL",new Vector3(0f, 0f, 0f));
		shapeAH.Add ("LipUpperR",new Vector3(0f, 0f, 0f));
		
		var shapeOO = new Dictionary<string, Vector3>();
		shapeOO.Add ("LipLowerR",new Vector3(0.002459016f, 0.00147541f, 0f));
		shapeOO.Add ("LipLowerL",new Vector3(0.002459016f, -0.00147541f, 0f));
		shapeOO.Add ("Jaw",new Vector3(0.0009836066f, 0f, 0f));
		shapeOO.Add ("LipCornerL",new Vector3(0.0009836066f, -0.007868852f, 0f));
		shapeOO.Add ("LipCornerR",new Vector3(0.0009836066f, 0.007868852f, 0f));
		shapeOO.Add ("LipUpperL",new Vector3(-0.0004918033f, -0.00147541f, 0f));
		shapeOO.Add ("LipUpperR",new Vector3(-0.0004918033f, 0.00147541f, 0f));
		
		var shapeBITE = new Dictionary<string, Vector3>();
		shapeBITE.Add ("LipLowerR",new Vector3(0f, 0f, 0.0004918033f));
		shapeBITE.Add ("LipLowerL",new Vector3(0f, 0f, 0.0004918033f));
		shapeBITE.Add ("Jaw",new Vector3(0.0009836066f, 0f, 0f));
		shapeBITE.Add ("LipCornerL",new Vector3(0f, 0f, 0f));
		shapeBITE.Add ("LipCornerR",new Vector3(0f, 0f, 0f));
		shapeBITE.Add ("LipUpperL",new Vector3(-0.001167213f, 0f, -0.00147541f));
		shapeBITE.Add ("LipUpperR",new Vector3(-0.001167213f, 0f, -0.00147541f));	
		
		var shapeEE = new Dictionary<string, Vector3>();
		shapeEE.Add ("LipLowerR",new Vector3(0.0004918033f, 0f, 0f));
		shapeEE.Add ("LipLowerL",new Vector3(0.0004918033f, 0f, 0f));
		shapeEE.Add ("Jaw",new Vector3(0.002459016f, 0f, 0f));
		shapeEE.Add ("LipCornerL",new Vector3(-0.002459016f, -0.0004918033f, 0f));
		shapeEE.Add ("LipCornerR",new Vector3(-0.002459016f, 0.0004918033f, 0f));
		shapeEE.Add ("LipUpperL",new Vector3(-0.0004918033f, 0f, 0f));
		shapeEE.Add ("LipUpperR",new Vector3(-0.0004918033f, 0f, 0f));
		
		var shapeEH = new Dictionary<string, Vector3>();
		shapeEH.Add ("LipLowerR",new Vector3(0.0009836066f, 0f, 0f));
		shapeEH.Add ("LipLowerL",new Vector3(0.0009836066f, 0f, 0f));
		shapeEH.Add ("Jaw",new Vector3(0.003934426f, 0f, 0f));
		shapeEH.Add ("LipCornerL",new Vector3(-0.001967213f, -0.001967213f, 0f));
		shapeEH.Add ("LipCornerR",new Vector3(-0.001967213f, 0.001967213f, 0f));
		shapeEH.Add ("LipUpperL",new Vector3(-0.0004918033f, 0f, 0f));
		shapeEH.Add ("LipUpperR",new Vector3(-0.0004918033f, 0f, 0f));
		
		var shapeIH = new Dictionary<string, Vector3>();
		shapeIH.Add ("LipLowerR",new Vector3(0.002459016f, 0f, 0f));
		shapeIH.Add ("LipLowerL",new Vector3(0.002459016f, 0f, 0f));
		shapeIH.Add ("Jaw",new Vector3(0.001967213f, 0f, 0f));
		shapeIH.Add ("LipCornerL",new Vector3(-0.00147541f, -0.00147541f, 0f));
		shapeIH.Add ("LipCornerR",new Vector3(-0.00147541f, 0.00147541f, 0f));
		shapeIH.Add ("LipUpperL",new Vector3(-0.0004918033f, 0f, 0f));
		shapeIH.Add ("LipUpperR",new Vector3(-0.0004918033f, 0f, 0f));
		
		lib = new Dictionary<string, Dictionary<string, Vector3>>();
		lib.Add ("o", shapeAW);
		lib.Add ("y", shapeEE);
		lib.Add ("e", shapeEH);
		lib.Add ("!", shapeTENSE);
		lib.Add ("=", shapeRELAXED);
		lib.Add ("v", shapeBITE);
		
		Populate ();
	}

}
