using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Allows for easy making of expressions and mouth shapes and copies the results formatted
/// as a dictionary of Vector3. Simply attach it to any GameObject (ScriptManager is an appropriate
/// place for it) and drag the head transform of the character you would like to manipulate into
/// the head variable in the inspector.
/// </summary>
public class FaceControllerGUI : MonoBehaviour {
	public GameObject AffectedAgent;	
	
	private Dictionary<string,Transform> bones;
	private Dictionary<string, Dictionary<string, float>> pose;
	private Dictionary<string, Vector3> origin;
	private bool reset;
	private bool copy;
	private bool mirroring = false;
	private bool mirror_toggle;
	private float min = -0.015f;
	private float max = 0.015f;
	private Vector2 _scroller = new Vector2();
	
	void Start () {
		origin = new Dictionary<string, Vector3>();
		bones = AffectedAgent.GetComponent<FaceControllerII>().GetBones();
		Debug.Log (bones.Count);
		foreach(Transform bone in bones.Values) {
			origin.Add (bone.name, bone.localPosition);
		}
		InitPose ();
	}
	
	void InitPose() {		
		pose = new Dictionary<string, Dictionary<string, float>>();
		mirroring = false;	
		foreach(var bone in origin.Keys) {
			var temp = new Dictionary<string, float>();
			temp.Add ("X", 0f);
			temp.Add ("Y", 0f);
			temp.Add ("Z", 0f);
			pose.Add (bone, temp);
		}
	}

	void MirrorPose() {
		foreach(string boneL in pose.Keys) {
			if(boneL.EndsWith ("L")) {
				var boneR = boneL.Substring(0,boneL.Length-1) + "R";
				pose[boneR]["X"] = pose[boneL]["X"];
				pose[boneR]["Y"] = -pose[boneL]["Y"];
				pose[boneR]["Z"] = pose[boneL]["Z"];
			}
		}
	}
	
	void OnGUI () {
		//Make automatic layout area
		GUILayout.BeginArea(new Rect(0,0,250, 2*Screen.height/3));
		_scroller = GUILayout.BeginScrollView(_scroller, GUI.skin.GetStyle("Box"));
		GUILayout.BeginVertical();
		mirror_toggle = GUILayout.Button ("Toggle mirroring");		
		foreach(string bone in pose.Keys) {
			if(mirroring && (bone.EndsWith ("R") && !bone.Contains ("Tongue"))) continue;
			GUILayout.Label (bone);
			GUILayout.BeginHorizontal ();
			pose[bone]["X"] = GUILayout.HorizontalSlider(pose[bone]["X"], min, max);
			pose[bone]["Y"] = GUILayout.HorizontalSlider(pose[bone]["Y"], min, max);
			pose[bone]["Z"] = GUILayout.HorizontalSlider(pose[bone]["Z"], min, max);
			GUILayout.EndHorizontal();
		}
		reset = GUILayout.Button ("Reset");
		copy = GUILayout.Button ("Copy to clipboard");
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
	
	void LateUpdate () {	
		if(GUI.changed) {
			if(reset) {
				InitPose();
			}
			if(copy) {
				CopyPose();
			}
			if(mirror_toggle) {
				mirroring = !mirroring;
			}
			if(mirroring) {
				MirrorPose ();
			}
			ShowExpression();
		}
		
	}
	
	void CopyPose() {
		string str = "<EXPRESSION_NAME_HERE>"+System.Environment.NewLine;
		foreach (string bone in pose.Keys) {
			if(pose[bone]["X"] != 0f || pose[bone]["Y"] != 0f || pose[bone]["Z"] != 0f) {
				str+=bone+","+pose[bone]["X"]+","+pose[bone]["Y"]+","+pose[bone]["Z"]+System.Environment.NewLine;
			}
		}		
		TextEditor te = new TextEditor();
		te.content = new GUIContent(str);
		te.SelectAll();
		te.Copy();
	}
	
	void ShowExpression() {
		foreach(Transform bone in bones.Values) {
			bone.localPosition = origin[bone.name] + new Vector3(pose[bone.name]["X"],pose[bone.name]["Y"],pose[bone.name]["Z"]);
		}
	}
	
}
