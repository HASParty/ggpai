using UnityEngine;
using System.Collections;

public class TestManager : Singleton<TestManager> {
    public MouseCast MouseCast;
    public GameObject[] HideMe;
    public AudioSource silenceMe;

    public bool HidePlayerMode = false;
    private bool showingPlayer = true;

    void Update() {
        if(showingPlayer && HidePlayerMode) {
            HidePlayer();
        }
        if(!showingPlayer && !HidePlayerMode) {
            ShowPlayer();
        }
    }

    public void HidePlayer() {
        Debug.Log("toggled test mode on");
        showingPlayer = false;
        foreach (GameObject go in HideMe) {
            go.SetActive(false);
            silenceMe.mute = true;
        }
        //MouseCast.FreeMode();
    }

    public void ShowPlayer() {
        Debug.Log("toggled test mode off");
        showingPlayer = true;
        foreach (GameObject go in HideMe) {
            go.SetActive(true);
            silenceMe.mute = false;
        }
       // MouseCast.DefaultMode();
    }
}
