using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public struct IKTarget
{
    public Transform Target;
    public float Effect;
}

public class ActorMotion : MonoBehaviour {
    private OpenHeadLookController headlook;
    private Animator animator;
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
        animator = GetComponent<Animator>();
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

        LerpIK(leftHand);
        LerpIK(rightHand);

    }

    //IK

    class IKLookAt
    {
        public IKLookAt(AvatarIKGoal avatarIKGoal)
        {
            IKGoal = avatarIKGoal;
        }
        public float Weight = 0f;
        public float FinalWeight = 0f;
        public float Elapsed = 0f;
        public GameObject Target;
        public float TargetHeight = 0f;
        public AvatarIKGoal IKGoal;
        public bool Active = false;
    }

    IKLookAt leftHand;
    IKLookAt rightHand;
    public Transform LeftHand;
    public Transform RightHand;
    public Transform LeftHandHolding;
    public Transform RightHandHolding;
    void OnAnimatorIK()
    {
        if (leftHand == null)
        {
            leftHand = new IKLookAt(AvatarIKGoal.LeftHand);
            rightHand = new IKLookAt(AvatarIKGoal.RightHand);
        }

        UpdateIK(leftHand);
        UpdateIK(rightHand);

    }

    void LerpIK(IKLookAt ik)
    {
        if (ik == null)
            return;
        if (ik.Weight < 0.2f && ik.Elapsed <= 1f && ik.Active)
        {
            //Debug.Log ("up "+ ik.Weight);
            ik.Weight = Mathf.Lerp(0, 0.2f, ik.Elapsed / 1f);
            ik.FinalWeight = ik.Weight;
            ik.Elapsed += Time.deltaTime;
        }
        else if (ik.Weight > 0f && ik.Elapsed <= 1f && !ik.Active)
        {
            //Debug.Log ("down "+ ik.Weight);
            ik.Weight = Mathf.Lerp(ik.FinalWeight, 0, ik.Elapsed / 1f);
            ik.Elapsed += Time.deltaTime;
        }
        else if (!ik.Active)
        {
            ik.Weight = 0f;
        }
    }

    void UpdateIK(IKLookAt ik)
    {
        if (ik == null)
            return;
        animator.SetIKPositionWeight(ik.IKGoal, ik.Weight);
        Vector3 curTarget;
        if (ik.Target == null)
        {
            curTarget = targetPoint;
        }
        else
        {
            curTarget = ik.Target.transform.position + new Vector3(0, ik.TargetHeight, 0);
        }
        animator.SetIKPosition(ik.IKGoal, curTarget);
    }

    public void Grab(float duration, GameObject target, bool left = true)
    {
        IKLookAt ik = (left ? leftHand : rightHand);
        if (left)
        {
            animator.SetTrigger("Reach");
        }
        else
        {
            //animator.SetInteger("Point", 1);
        }

        ik.Weight = 0f;
        ik.Elapsed = 0f;
        ik.Active = true;
        ik.Target = target;
        ik.TargetHeight = 0f;
        StartCoroutine(FinishGrab(duration, ik, left));
    }

    public void Place(float duration, GameObject where, bool left = true)
    {
        Transform what = (left ? LeftHandHolding : RightHandHolding);
        if(what == null) Debug.LogWarning("What should I place");
        else
        {
            IKLookAt ik = (left ? leftHand : rightHand);
            if (left)
            {
                animator.SetTrigger("Reach");
            }
            else
            {
                //animator.SetInteger("Point", 1);
            }

            ik.Weight = 0f;
            ik.Elapsed = 0f;
            ik.Active = true;
            ik.Target = where;
            ik.TargetHeight = 0f;
            StartCoroutine(FinishPlace(what, duration, ik, left));
        }
    }

    IEnumerator FinishPlace(Transform placeMe, float duration, IKLookAt ik, bool left = true)
    {
        yield return new WaitForSeconds(duration);
        ik.Elapsed = 0f;
        placeMe.SetParent(ik.Target.transform);
        if (left)
        {
            LeftHandHolding = null;
        }
        else
        {
            RightHandHolding = null;
        }
        ik.Active = false;
        yield return null;
    }

    IEnumerator FinishGrab(float duration, IKLookAt ik, bool left = true)
    {
        yield return new WaitForSeconds(duration);
        ik.Elapsed = 0f;
        if (left)
        {
            ik.Target.transform.SetParent(LeftHand);
            LeftHandHolding = ik.Target.transform;
        }
        else
        {
            ik.Target.transform.SetParent(RightHand);
            RightHandHolding = ik.Target.transform;
        }
        ik.Active = false;
        yield return null;
    }

    public void Point(float duration, GameObject target, bool lookAtTarget, bool left = true)
    {
        IKLookAt ik = (left ? leftHand : rightHand);
        if (left)
        {
            animator.SetTrigger("Reach");
        }
        else
        {
            //animator.SetInteger("Point", 1);
        }

        if (ik.Active || ik.Weight > 0f)
            return;
        //TODO: trigger appropriate hand animation for pointing
        ik.Weight = 0f;
        ik.Elapsed = 0f;
        ik.Active = true;
        ik.Target = target;
        ik.TargetHeight = 0f;
        StartCoroutine(StopPointing(duration, ik));
    }

    IEnumerator StopPointing(float duration, IKLookAt ik)
    {
        yield return new WaitForSeconds(duration);
        ik.Elapsed = 0f;
        //animator.SetInteger("Point", 0);
        ik.Active = false;
        yield return null;
    }


}
