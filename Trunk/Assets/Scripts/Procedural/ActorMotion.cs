using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct IKTarget
{
    public Transform Target;
    public float Effect;
}

public class ActorMotion : MonoBehaviour {
    private OpenHeadLookController headlook;
    public bool isLooking = false;

    public float headLookEffectDampTime = 2f;
    private Transform targetObject;
    private Vector3 targetPoint = Vector3.zero;

    public GameObject Table;
    public GameObject Player;

    private bool usingObj = false;

    [Header("IK targets")]
    public IKTarget LeftMidThigh;
    public IKTarget RightMidThigh;
    public IKTarget LeftShoulder;
    public IKTarget RightShoulder;
    public IKTarget LeftNeck;
    public IKTarget RightNeck;
    public IKTarget Nose;
    public IKTarget Chin;

	// Use this for initialization
	void Start () {
        headlook = GetComponent<OpenHeadLookController>();
        SetTarget(Table.transform);
	}

    private float desiredHlEffect = 0f;
    private float prevHlEffect = 0f;
    private float elapsed = 0f;

    public void SetTarget(Vector3 pos) {
        targetPoint = pos;
        usingObj = false;
    }

    public void SetTarget(Transform transform) {
        targetObject = transform;
        usingObj = true;
    }

    public Vector3 GetTarget() {
        if (usingObj) return targetObject.position;
        return targetPoint;
    }
    

    public void SetHeadLookEffect(float val, float dampTime, float deltaTime)
    {
        val = Mathf.Clamp01(val);

        if (desiredHlEffect != val)
        {
            prevHlEffect = headlook.effect;
            desiredHlEffect = val;
            elapsed = 0f;
        }

        if (headlook.effect != desiredHlEffect)
        {
            elapsed += deltaTime;
            headlook.effect = Mathf.SmoothStep(prevHlEffect, desiredHlEffect, elapsed / dampTime);
        }
    }

    // Update is called once per frame
    void Update () {
        if (isLooking)
        {
            headlook.SetTarget(GetTarget());
            SetHeadLookEffect(1f, headLookEffectDampTime, Time.deltaTime);
        }
        else
        {
            SetHeadLookEffect(0f, headLookEffectDampTime, Time.deltaTime);
        }
    }


}
