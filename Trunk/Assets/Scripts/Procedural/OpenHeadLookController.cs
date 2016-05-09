using UnityEngine;
using System.Collections;
using Impulsion;

/// <summary>
/// Open head look controller. Subclass of HeadLookController, allows
/// for a variety of more options and control.
/// </summary>
[RequireComponent(typeof(AgentBoneFinder))]
public class OpenHeadLookController : HeadLookController
{
    public float lean = 0;
    public int leanAffectors = 0;
    public float headNod = 0;
    public float headShake = 0;

    public bool headOverride = false;
    public bool eyeOverride = false;

    private AgentBoneFinder _abf;

    public void Awake()
    {
        _abf = transform.GetComponent<AgentBoneFinder>();
        AutoPopulateSegments();
    }

    #region Segment populater
    public void AutoPopulateSegments()
    {
        segments = new BendingSegment[5];
        nonAffectedJoints = new Impulsion.NonAffectedJoints[2];

        for (int i = 0; i < 5; i++)
            segments[i] = new BendingSegment();

        for (int i = 0; i < 2; i++)
            nonAffectedJoints[i] = new Impulsion.NonAffectedJoints();

        nonAffectedJoints[0].joint = _abf.findBone("LeftArm");
        nonAffectedJoints[0].effect = 0.4f;
        nonAffectedJoints[1].joint = _abf.findBone("RightArm");
        nonAffectedJoints[1].effect = 0.4f;

        segments[0].firstTransform = _abf.findBone("Spine");
        segments[0].lastTransform = _abf.findBone("Spine1");
        segments[0].thresholdAngleDifference = 45f;
        segments[0].bendingMultiplier = 0.6f;
        segments[0].maxAngleDifference = 30f;
        segments[0].maxBendingAngle = 10f;
        segments[0].responsiveness = 0.5f;
        segments[0].leanEffect = 0.5f;

        segments[1].firstTransform = segments[0].lastTransform;
        segments[1].lastTransform = _abf.findBone("Spine2");
        segments[1].thresholdAngleDifference = 30f;
        segments[1].bendingMultiplier = 0.6f;
        segments[1].maxAngleDifference = 90f;
        segments[1].maxBendingAngle = 20f;
        segments[1].responsiveness = 0.5f;
        segments[1].leanEffect = 0.5f;

        segments[2].firstTransform = _abf.findBone("Neck");
        segments[2].lastTransform = _abf.findBone("Head");
        segments[2].isHead = true;
        segments[2].thresholdAngleDifference = 15f;
        segments[2].bendingMultiplier = 0.7f;
        segments[2].maxAngleDifference = 75f;
        segments[2].maxBendingAngle = 65f;
        segments[2].responsiveness = 2f;
        segments[2].leanEffect = 0f;

        segments[3].firstTransform = _abf.findBone("LeftEye");
        segments[3].lastTransform = segments[3].firstTransform;
        segments[3].isEye = true;
        segments[3].thresholdAngleDifference = 0f;
        segments[3].bendingMultiplier = 1f;
        segments[3].maxAngleDifference = 0f;
        segments[3].maxBendingAngle = 15f;
        segments[3].responsiveness = 20f;
        segments[3].leanEffect = 0f;

        segments[4].firstTransform = _abf.findBone("RightEye");
        segments[4].lastTransform = segments[4].firstTransform;
        segments[4].isEye = true;
        segments[4].thresholdAngleDifference = 0f;
        segments[4].bendingMultiplier = 1f;
        segments[4].maxAngleDifference = 0f;
        segments[4].maxBendingAngle = 15f;
        segments[4].responsiveness = 20f;
        segments[4].leanEffect = 0f;

    }
    #endregion

    void Start()
    {
        if (rootNode == null)
        {
            rootNode = transform;
        }

        // Setup segments
        foreach (BendingSegment segment in segments)
        {

            Quaternion parentRot = segment.firstTransform.parent.rotation;
            Quaternion parentRotInv = Quaternion.Inverse(parentRot);
            segment.referenceLookDir =
                parentRotInv * rootNode.rotation * headLookVector.normalized;
            segment.referenceUpDir =
                parentRotInv * rootNode.rotation * headUpVector.normalized;
            segment.angleH = 0;
            segment.angleV = 0;
            segment.dirUp = segment.referenceUpDir;

            segment.chainLength = 1;
            Transform t = segment.lastTransform;
            while (t != segment.firstTransform && t != t.root)
            {
                segment.chainLength++;
                t = t.parent;
            }

            segment.origRotations = new Quaternion[segment.chainLength];
            t = segment.lastTransform;
            for (int i = segment.chainLength - 1; i >= 0; i--)
            {
                segment.origRotations[i] = t.localRotation;
                t = t.parent;
            }
        }
    }

    void LateUpdate()
    {
        // Remember initial directions of joints that should not be affected
        Vector3[] jointDirections = new Vector3[nonAffectedJoints.Length];
        for (int i = 0; i < nonAffectedJoints.Length; i++)
        {
            foreach (Transform child in nonAffectedJoints[i].joint)
            {
                jointDirections[i] = child.position - nonAffectedJoints[i].joint.position;
                break;
            }
        }
        // Handle each segment
        foreach (BendingSegment segment in segments)
        {
            //segment.target = target;
            Transform t = segment.lastTransform;
            if (overrideAnimation)
            {
                for (int i = segment.chainLength - 1; i >= 0; i--)
                {
                    t.localRotation = segment.origRotations[i];
                    t = t.parent;
                }
            }

            Quaternion parentRot = segment.firstTransform.parent.rotation;
            Quaternion parentRotInv = Quaternion.Inverse(parentRot);

            // Desired look direction in world space
            Vector3 lookDirWorld = (segment.target - segment.lastTransform.position).normalized;

            // Desired look directions in neck parent space
            Vector3 lookDirGoal = (parentRotInv * lookDirWorld);

            // Get the horizontal and vertical rotation angle to look at the target
            float hAngle = AngleAroundAxis(
                segment.referenceLookDir, lookDirGoal, segment.referenceUpDir
                );

            Vector3 rightOfTarget = Vector3.Cross(segment.referenceUpDir, lookDirGoal);

            Vector3 lookDirGoalinHPlane =
                lookDirGoal - Vector3.Project(lookDirGoal, segment.referenceUpDir);

            float vAngle = AngleAroundAxis(
                lookDirGoalinHPlane, lookDirGoal, rightOfTarget
                );

            // Handle threshold angle difference, bending multiplier,
            // and max angle difference here
            float hAngleThr = Mathf.Max(
                0, Mathf.Abs(hAngle) - segment.thresholdAngleDifference
                ) * Mathf.Sign(hAngle);

            float vAngleThr = Mathf.Max(
                0, Mathf.Abs(vAngle) - segment.thresholdAngleDifference
                ) * Mathf.Sign(vAngle);

            hAngle = Mathf.Max(
                Mathf.Abs(hAngleThr) * Mathf.Abs(segment.bendingMultiplier),
                Mathf.Abs(hAngle) - segment.maxAngleDifference
                ) * Mathf.Sign(hAngle) * Mathf.Sign(segment.bendingMultiplier);

            vAngle = Mathf.Max(
                Mathf.Abs(vAngleThr) * Mathf.Abs(segment.bendingMultiplier),
                Mathf.Abs(vAngle) - segment.maxAngleDifference
                ) * Mathf.Sign(vAngle) * Mathf.Sign(segment.bendingMultiplier);
            /////////////////////////////////////////////////////////////////
            /// Lean and head control - hafdis13@ru.is                   ///
            ////////////////////////////////////////////////////////////////
            float leanMax = 0;
            //we don't want to mess with the head and eyes, the head is controlled separately
            if (!segment.isHead && !segment.isEye)
            {
                vAngle += lean * segment.leanEffect;
                //we want to be able to lean on the vertical axis more than the
                //script allows us, since this script has the main purpose of
                //controlling where the character is looking and doesn't want
                //unnatural bending. But we want to control how much the
                //character is bending forwards.
                if (segment.leanEffect > 0)
                {
                    //so if the lean is affected we calculate this
                    leanMax += lean * segment.leanEffect;
                    //and if this causes the maxBendingAngle to exceed 85 degrees
                    if (leanMax + segment.maxBendingAngle > 85)
                    {
                        //we make sure it maxes out at 85 degrees
                        leanMax = 85 - segment.maxBendingAngle;
                    }
                }
            }
            else if (segment.isHead)
            {
                //if we are dealing with the head, we want other parameters to affect it.
                vAngle += headNod - lean/2;
                hAngle += headShake;
            }
            ////////////////////////////////////////////////////////////////
            // Handle max bending angle here
            hAngle = Mathf.Clamp(hAngle, -segment.maxBendingAngle, segment.maxBendingAngle);
            vAngle = Mathf.Clamp(vAngle, -(segment.maxBendingAngle + leanMax), segment.maxBendingAngle + leanMax);

            Vector3 referenceRightDir =
                Vector3.Cross(segment.referenceUpDir, segment.referenceLookDir);
            // Lerp angles
            segment.angleH = Mathf.Lerp(
                segment.angleH, hAngle, Time.deltaTime * segment.responsiveness
                );
            segment.angleV = Mathf.Lerp(
                segment.angleV, vAngle, Time.deltaTime * segment.responsiveness
                );

            // Get direction
            lookDirGoal = Quaternion.AngleAxis(segment.angleH, segment.referenceUpDir)
                * Quaternion.AngleAxis(segment.angleV, referenceRightDir)
                    * segment.referenceLookDir;

            // Make look and up perpendicular
            Vector3 upDirGoal = segment.referenceUpDir;
            Vector3.OrthoNormalize(ref lookDirGoal, ref upDirGoal);

            // Interpolated look and up directions in neck parent space
            Vector3 lookDir = lookDirGoal;
            segment.dirUp = Vector3.Slerp(segment.dirUp, upDirGoal, Time.deltaTime * 5);
            Vector3.OrthoNormalize(ref lookDir, ref segment.dirUp);

            // Look rotation in world space
            Quaternion lookRot = (
                (parentRot * Quaternion.LookRotation(lookDir, segment.dirUp))
                * Quaternion.Inverse(
                parentRot * Quaternion.LookRotation(
                segment.referenceLookDir, segment.referenceUpDir
                )
                )
                );

            // Distribute rotation over all joints in segment
            Quaternion dividedRotation =
                Quaternion.Slerp(Quaternion.identity, lookRot, effect / segment.chainLength);
            t = segment.lastTransform;
            for (int i = 0; i < segment.chainLength; i++)
            {
                t.rotation = dividedRotation * t.rotation;
                t = t.parent;
            }


        }

        // Handle non affected joints
        for (int i = 0; i < nonAffectedJoints.Length; i++)
        {
            Vector3 newJointDirection = Vector3.zero;

            foreach (Transform child in nonAffectedJoints[i].joint)
            {
                newJointDirection = child.position - nonAffectedJoints[i].joint.position;
                break;
            }

            Vector3 combinedJointDirection = Vector3.Slerp(
                jointDirections[i], newJointDirection, nonAffectedJoints[i].effect
                );

            nonAffectedJoints[i].joint.rotation = Quaternion.FromToRotation(
                newJointDirection, combinedJointDirection
                ) * nonAffectedJoints[i].joint.rotation;
        }
    }

    /// <summary>
    /// Sets the target. More complicated variation. Can set a duration, as well.
    /// </summary>
    /// <param name="targetPoint">Target point.</param>
    /// <param name="headOnly">If set to <c>true</c>, the agent moves his head only.</param>
    /// <param name="eyesOnly">If set to <c>true</c>, the agent moves his eyes only.</param>
    /// <param name="duration">Duration.</param>
    public void SetTarget(Vector3 targetPoint, bool headOnly = false, bool eyesOnly = false, float duration = 0f)
    {
        currTargetSet++;
        int id = currTargetSet;        
        if (duration > 0f)
        {
            if (!headOverride)
            {
                headOverride = headOnly;
            }
            if (!eyeOverride)
            {
                eyeOverride = eyesOnly;
            }
            StartCoroutine(ResetTarget(target, duration, headOnly, eyesOnly, id));
        }
        target = targetPoint;
        foreach (BendingSegment segment in segments)
        {
            if ((!headOnly && !eyesOnly) || (headOnly && segment.isHead) || (eyesOnly && segment.isEye))
            {
                if (!((!headOnly && headOverride && segment.isHead) || (!eyesOnly && eyeOverride && segment.isEye)))
                {
                    segment.target = targetPoint;
                }
            }
        }
    }
    int currTargetSet = 0;

    IEnumerator ResetTarget(Vector3 previousTarget, float duration, bool headOverridden, bool eyeOverridden, int id)
    {
        yield return new WaitForSeconds(duration);
        if (currTargetSet == id) {
            if (headOverridden) {
                headOverride = false;
            }
            if (eyeOverridden) {
                eyeOverride = false;
            }
            SetTarget(previousTarget);
        }
    }

    
}