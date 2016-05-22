using UnityEngine;
using System.Collections;
using Boardgame;
using Boardgame.Networking;

public class MouseCast : MonoBehaviour {

    PhysicalCell cell;
    // Use this for initialization
    void Start() {
        transform.position = Vector3.zero;
        transform.up = Vector3.forward;
    }

    // Update is called once per frame
    void LateUpdate() {
        if (cell != null && (Input.GetKeyUp(KeyCode.JoystickButton0) || Input.GetKeyUp(KeyCode.Space))) {
            cell.OnSelect();
        }

        if (Input.GetKeyUp(KeyCode.C) || (Input.GetKeyUp(KeyCode.JoystickButton6))) {
            UnityEngine.VR.InputTracking.Recenter();
        }

        if (Input.GetKeyUp(KeyCode.T)) {
            TestManager.Instance.HidePlayerMode = !TestManager.Instance.HidePlayerMode;
        }

        if (Input.GetKeyUp(KeyCode.Escape)) {
            ConnectionMonitor.Instance.EndGame();
        }

        if (ConnectionMonitor.Instance.IsOver) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }

    public void DefaultMode() {
        defaultMode = true;
    }

    public void FreeMode() {
        defaultMode = false;
    }

    bool defaultMode = true;

    Ray ray;
    void FixedUpdate() {
        Ray ray;
        if (defaultMode) {
            ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.4f, 0f));
        } else {
            if (!Camera.current) return;
            ray = Camera.current.ScreenPointToRay(Input.mousePosition);
        }
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.9f)) {
            var newCell = hit.collider.gameObject.GetComponent<PhysicalCell>();
            if (newCell != null) {
                if (cell != null) cell.OnLookAway();
                newCell.OnLookAt();
                cell = newCell;
            }
            transform.position = new Vector3(hit.point.x, hit.point.y + 0.002f, hit.point.z);
        } else {
            transform.position = Vector3.zero;
        }
    }
}
