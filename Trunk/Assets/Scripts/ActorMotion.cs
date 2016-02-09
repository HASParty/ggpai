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
    public Transform targetObject;

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
	}

    private float desiredHlEffect = 0f;
    private float prevHlEffect = 0f;
    private float elapsed = 0f;

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
            headlook.SetTarget(targetObject.position);
            SetHeadLookEffect(1f, headLookEffectDampTime, Time.deltaTime);
        }
        else
        {
            SetHeadLookEffect(0f, headLookEffectDampTime, Time.deltaTime);
        }
    }


}
