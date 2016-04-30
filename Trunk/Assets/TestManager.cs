using UnityEngine;
using System.Collections;

public class TestManager : Singleton<TestManager> {
    public Camera Overhead;
    public Camera Player;
    public MouseCast MouseCast;
    public GameObject[] HideMe;

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
        Player.gameObject.SetActive(false);
        Overhead.gameObject.SetActive(true);
        Camera.SetupCurrent(Overhead);
        foreach (GameObject go in HideMe) {
            go.SetActive(false);
        }
        MouseCast.FreeMode();
    }

    public void ShowPlayer() {
        Debug.Log("toggled test mode off");
        showingPlayer = true;
        Player.gameObject.SetActive(true);
        Overhead.gameObject.SetActive(false);
        Camera.SetupCurrent(Player);
        foreach (GameObject go in HideMe) {
            go.SetActive(true);
        }
        MouseCast.DefaultMode();
    }
}
