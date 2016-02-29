using UnityEngine;
using System.Collections.Generic;
using Behaviour;
using Boardgame.UI;

public class DebugManager : MonoBehaviour {

    public Transform BMLDebugPosition;
    public GameObject BMLScheduleDebuggerPrefab;
    public Transform MoodDebugPosition;
    public GameObject MoodDebuggerPrefab;
    public Canvas canvas;
	//etc
	BMLScheduleDebugger bmlDebug;
    UIMoodViewer mood;
    
    List<GameObject> hiddenCache = new List<GameObject> ();

    public Vector3 scale;

	private static DebugManager _instance;
	public static DebugManager Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject go = new GameObject("DebugManager");
				_instance = go.AddComponent<DebugManager>();
				_instance.Init ();
			}
			return _instance;
		}
	}

	public void Hide() {
		canvas.enabled = false;
	}

	public void Show() {
		canvas.enabled = true;
	}

    void Update() {
        if(Input.GetKeyUp(KeyCode.L)) {
            if (canvas.enabled) Hide();
            else Show();
        }
    }

	// Use this for initialization
	void Awake () {

        if (_instance == null)
        {
            _instance = this;
			_instance.Init ();
        }
        else if (_instance != this)
        {
            Debug.LogWarning("Second DebugManager created, deleting this script!");
            Destroy(this);
        }
	}

	void Init() {
		bmlDebug = Instantiate (BMLScheduleDebuggerPrefab).GetComponent<BMLScheduleDebugger> ();
		bmlDebug.transform.SetParent (canvas.transform);
		bmlDebug.GetComponent<RectTransform> ().localScale = scale;
        bmlDebug.transform.position = BMLDebugPosition.position;
        bmlDebug.transform.rotation = BMLDebugPosition.rotation;
        mood = Instantiate(MoodDebuggerPrefab).GetComponent<UIMoodViewer>();
        mood.transform.SetParent(canvas.transform);
        mood.GetComponent<RectTransform>().localScale = scale;
        mood.transform.position = MoodDebugPosition.position;
        mood.transform.rotation = MoodDebugPosition.rotation;
        canvas.enabled = false;
	}

	public void OnChunkStart(BMLChunk chunk) {
		bmlDebug.Spawn (chunk);
	}
}
