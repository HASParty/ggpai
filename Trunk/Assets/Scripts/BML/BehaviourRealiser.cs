using UnityEngine;
using System.Collections;
using Behaviour;
using System;
using System.Collections.Generic;

public class BehaviourRealiser : MonoBehaviour {
    OpenHeadLookController _hl;
    FaceControllerII _fc;
    ActorMotion _motion;

    public void ScheduleBehaviour(BMLBody body) {
        StartCoroutine(ScheduleBehaviourBody(body));
    }

    IEnumerator ScheduleBehaviourBody(BMLBody body) {
        //wait until my body is ready
        while (!body.IsReady() || !body.Synchronized()) {
            yield return new WaitForEndOfFrame();
        }
        body.syncComplete = true;
        foreach (BMLChunk chunk in body.Chunks.Values) {
            ScheduleBehaviour(chunk);
        }
        yield return new WaitForSeconds(body.latestEnd);
        body.isDone = true;
    }

    List<BMLChunk> SyncChunks(Dictionary<string, KeyValuePair<float, SyncPoints>> syncs, Dictionary<string, BMLChunk> chunks) {
        foreach (string id in syncs.Keys) {
            if (chunks.ContainsKey(id)) {
                chunks[id].Sync(syncs[id].Value, syncs[id].Key);
            }
        }
        return new List<BMLChunk>(chunks.Values);
    }

    public void ScheduleBehaviour(BMLChunk chunk) {
        //Debug.Log ("executing " + chunk.ID + " at " + Time.time + " in " + chunk.Start);
        switch (chunk.Type) {
            case BMLChunkType.Face:
                StartCoroutine(Schedule(chunk as Face));
                break;
            case BMLChunkType.FaceEmotion:
                StartCoroutine(Schedule(chunk as FaceEmotion));
                break;
            case BMLChunkType.Gaze:
                StartCoroutine(Schedule(chunk as Gaze));
                break;
            case BMLChunkType.GazeShift:
                StartCoroutine(Schedule(chunk as GazeShift));
                break;
            case BMLChunkType.Gesture:
                StartCoroutine(Schedule(chunk as Gesture));
                break;
            case BMLChunkType.Pointing:
                StartCoroutine(Schedule(chunk as Pointing));
                break;
            case BMLChunkType.Grasping:
                StartCoroutine(Schedule(chunk as Grasp));
                break;
            case BMLChunkType.Placing:
                StartCoroutine(Schedule(chunk as Place));
                break;
            case BMLChunkType.Head:
                StartCoroutine(Schedule(chunk as Head));
                break;
            case BMLChunkType.Locomotion:
                StartCoroutine(Schedule(chunk as Locomotion));
                break;
            case BMLChunkType.Posture:
                StartCoroutine(Schedule(chunk as Posture));
                break;
            case BMLChunkType.Speech:
                StartCoroutine(Schedule(chunk as Speech));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    IEnumerator Schedule(Face chunk) {
        yield return new WaitForSeconds(chunk.Start);
        DebugManager.Instance.OnChunkStart(chunk);
        string name = chunk.Lexeme.ToString();
        float transitionTo = chunk.AttackPeak;
        float duration = chunk.Relax - chunk.AttackPeak;
        float transitionFrom = chunk.End - chunk.Relax;
        var node = FaceControllerII.GenerateExpressionNode(name, transitionTo, transitionFrom, duration, modifier: chunk.Modifier + 1);
        Debug.Log("scheduling expression " + name + " for " + gameObject.name);
        _fc.ScheduleExpression(node);
        yield return null;
    }

    IEnumerator Schedule(FaceEmotion chunk) {
        yield return new WaitForSeconds(chunk.Start);
        DebugManager.Instance.OnChunkStart(chunk);
        
        var node = FaceControllerII.GenerateEmotionalExpression(chunk.Arousal, chunk.Valence, 0.75f);
        //Debug.Log("scheduling emotional expression for " + gameObject.name);
        _fc.ScheduleExpression(node);
        yield return null;
    }

    IEnumerator Schedule(Gaze chunk) {
        yield return new WaitForSeconds(chunk.Start);
        DebugManager.Instance.OnChunkStart(chunk);
        float duration = chunk.End;
        //ignores ready and relax for now
        if (chunk.Target == null) {
            //we actually just want to glance away rather than look at anything specific
            switch (chunk.Influence) {
                case Behaviour.Lexemes.Influence.EYES:
                    StartCoroutine(GlanceAway(duration));
                    break;
                case Behaviour.Lexemes.Influence.HEAD:
                    StartCoroutine(GlanceAway(duration, affectHead: true));
                    break;
                case Behaviour.Lexemes.Influence.WAIST:
                    StartCoroutine(GazeRandom(duration, returnToPreviousTarget: true));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        } else {
            //we have a specific target in mind
            switch (chunk.Influence) {
                case Behaviour.Lexemes.Influence.EYES:
                    StartCoroutine(Glance(chunk.Target, duration));
                    break;
                case Behaviour.Lexemes.Influence.HEAD:
                    StartCoroutine(Glance(chunk.Target, duration, affectHead: true));
                    break;
                case Behaviour.Lexemes.Influence.WAIST:
                    StartCoroutine(Gaze(chunk.Target, duration, returnToPreviousTarget: true));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        yield return null;
    }

    IEnumerator Schedule(GazeShift chunk) {
        yield return new WaitForSeconds(chunk.Start);
        DebugManager.Instance.OnChunkStart(chunk);
        float duration = chunk.End;
        if (chunk.Target == null) {
            StartCoroutine(GazeRandom(duration, returnToPreviousTarget: false));
        } else {
            StartCoroutine(Gaze(chunk.Target, duration, returnToPreviousTarget: false));
        }
        yield return null;
    }

    IEnumerator Schedule(Gesture chunk) {
        yield return new WaitForSeconds(chunk.Start);
        DebugManager.Instance.OnChunkStart(chunk);
        switch (chunk.Lexeme) {
            case Behaviour.Lexemes.Gestures.STROKE:
                // _motion.OpenPalm();
                break;
            case Behaviour.Lexemes.Gestures.REQUEST:
                //  _motion.Request();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        yield return null;
    }

    IEnumerator Schedule(Head chunk) {
        yield return new WaitForSeconds(chunk.Start);
        DebugManager.Instance.OnChunkStart(chunk);
        float duration = chunk.End;
        switch (chunk.Lexeme) {
            case Behaviour.Lexemes.Head.SHAKE:
                StartCoroutine(ShakeHead(duration, chunk.Repetition, chunk.Amount));
                break;
            case Behaviour.Lexemes.Head.NOD:
                StartCoroutine(NodDown(duration, chunk.Repetition, chunk.Amount));
                break;
            case Behaviour.Lexemes.Head.ACK:
                StartCoroutine(NodUp(duration, chunk.Repetition, chunk.Amount));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        yield return null;
    }

    IEnumerator Schedule(Pointing chunk) {
        yield return new WaitForSeconds(chunk.Start);
        DebugManager.Instance.OnChunkStart(chunk);
        float duration = chunk.End;
        StartCoroutine(Point(duration, chunk.Target, lookAtTarget: false));
    }

    IEnumerator Schedule(Grasp chunk)
    {
        yield return new WaitForSeconds(chunk.Start);
        DebugManager.Instance.OnChunkStart(chunk);
        float duration = chunk.End;
        bool left = chunk.Mode == Behaviour.Lexemes.Mode.LEFT_HAND;
        ActorMotion.Arm which = left ? _motion.Left : _motion.Right;
        yield return StartCoroutine(Grasp(duration, chunk.Target, left));
        chunk.Callback(which);

    }

    IEnumerator Schedule(Place chunk)
    {
        yield return new WaitForSeconds(chunk.Start);
        DebugManager.Instance.OnChunkStart(chunk);
        float duration = chunk.End;
        bool left = chunk.Mode == Behaviour.Lexemes.Mode.LEFT_HAND;
        GameObject which = left ? _motion.Left.holding.gameObject : _motion.Right.holding.gameObject;
        yield return StartCoroutine(Place(duration, chunk.Target, chunk.Mode == Behaviour.Lexemes.Mode.LEFT_HAND));
        chunk.Callback(which);
    }

    IEnumerator Schedule(Posture chunk) {
        yield return new WaitForSeconds(chunk.Start);
        DebugManager.Instance.OnChunkStart(chunk);
        float duration = chunk.End;
        //ignore stance for now
        foreach (Pose pose in chunk.Poses) {
            //ignoring affected parts
            switch (pose.Lexeme) {
                case Behaviour.Lexemes.BodyPose.ARMS_AKIMBO:
                    break;
                case Behaviour.Lexemes.BodyPose.ARMS_CROSSED:
                    StartCoroutine(Pose(1, 1, duration));
                    break;
                case Behaviour.Lexemes.BodyPose.ARMS_NEUTRAL:
                    break;
                case Behaviour.Lexemes.BodyPose.ARMS_OPEN:
                    break;
                case Behaviour.Lexemes.BodyPose.LEGS_CROSSED:
                    break;
                case Behaviour.Lexemes.BodyPose.LEGS_NEUTRAL:
                    break;
                case Behaviour.Lexemes.BodyPose.LEGS_OPEN:
                    break;
                case Behaviour.Lexemes.BodyPose.LEANING_FORWARD:
                    StartCoroutine(LeanIn(duration, pose.Degree));
                    break;
                case Behaviour.Lexemes.BodyPose.FIST_COVER_MOUTH:
                    StartCoroutine(Pose(0, 2, duration));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    IEnumerator Pose(int left, int right, float duration)
    {
        _motion.SetPose(left, right);
        yield return new WaitForSeconds(duration);
        if (_motion.IsPose(left, right))
        {
            _motion.SetPose(0, 0);
        }
    }

    IEnumerator Schedule(Locomotion chunk) {
        DebugManager.Instance.OnChunkStart(chunk);
        yield return null;
    }

    IEnumerator Schedule(Speech chunk) {
        yield return null;
    }

    void Start() {
        _hl = transform.GetComponent<OpenHeadLookController>();
        _fc = GetComponent<FaceControllerII>();
        _motion = GetComponent<ActorMotion>();
    }

    #region Behaviour building blocks
    /// <summary>
    /// Point for the specified duration. Might be changed so that the exact timing of the beat of the point can
    /// be set, and duration is simply for how long the actor keeps pointing at the target.
    /// </summary>
    /// <param name="duration">Duration of point.</param>
    /// <param name="lookAtTarget">If set to <c>true</c> look at target as well as point.</param>
    IEnumerator Point(float duration = 3f, GameObject target = null, bool lookAtTarget = true) {
        Debug.Log("Point");
        if (target != null) {
            //float angle = SignedAngle(transform.forward * 5, target.transform.position - transform.position, transform.up);
            //if (angle <= 85 && angle >= -85) {
                 _motion.Point(duration, target, lookAtTarget, true);
           /* } else {
                if (angle < 0) {
                    //  _motion.OpenPalmLeft();
                } else {
                    // _motion.OpenPalm();
              *  }
            }*/
        }
        yield return new WaitForSeconds(duration);
    }

    /// <summary>
    /// Grasp target taking time duration to do so.
    /// </summary>
    /// <param name="duration">Duration of motion.</param>
    IEnumerator Grasp(float duration = 3f, GameObject target = null, bool left = true)
    {
        Debug.Log("Grasp");
        if (target != null)
        {
            yield return StartCoroutine(_motion.Grab(duration, target, left));
            
        }
    }

    /// <summary>
    /// Place whatever is in hand at target taking time duration to do so.
    /// </summary>
    /// <param name="duration">Duration of motion.</param>
    IEnumerator Place(float duration = 3f, GameObject target = null, bool left = true)
    {
        Debug.Log("Place");
        if (target != null)
        {
            yield return _motion.Place(duration, target, left);
        }
    }

    float SignedAngle(Vector3 fromVector, Vector3 toVector, Vector3 upVector) {
        fromVector.y = 0;
        toVector.y = 0;
        // If the vector the angle is being calculated to is 0...
        if (toVector == Vector3.zero)
            // ... the angle between them is 0.
            return 0f;

        // Create a float to store the angle between the facing of the enemy and the direction it's travelling.
        float angle = Vector3.Angle(fromVector, toVector);

        // Find the cross product of the two vectors (this will point up if the velocity is to the right of forward).
        Vector3 normal = Vector3.Cross(fromVector, toVector);

        // The dot product of the normal with the upVector will be positive if they point in the same direction.
        return angle * Mathf.Sign(Vector3.Dot(normal, upVector));
    }

    /// <summary>
    /// Has the actor lean forwards.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="amount">Amount.</param>
    IEnumerator LeanIn(float duration = 3f, float amount = 30f) {
        Debug.Log("Leaning in");
        _hl.lean = amount;
        yield return new WaitForSeconds(duration);
        _hl.lean = 0;
    }
    /// <summary>
    /// Nod down. Duration/repetitions/amount are highly dependent on one another -
    /// the greater the duration, the closer the head gets to the actual amount, so
    /// if you have a lot of nods over a short period of time you might want to increase
    /// the amount so you don't just get a slightly vibrating head.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="repetitions">Repetitions.</param>
    /// <param name="amount">Amount.</param>
    IEnumerator NodDown(float duration = 0.5f, int repetitions = 1, float amount = 20f) {
        Debug.Log("NodDown");
        for (int i = 0; i < repetitions; i++) {
            _hl.headNod = amount;
            yield return new WaitForSeconds(duration / repetitions / 2);
            _hl.headNod = 0;
            yield return new WaitForSeconds(duration / repetitions / 2);
        }
    }

    /// <summary>
    /// Nod up. Same story as with NodDown
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="repetitions">Repetitions.</param>
    /// <param name="amount">Amount.</param>
    IEnumerator NodUp(float duration = 0.5f, int repetitions = 1, float amount = 10f) {
        Debug.Log("NodUp");
        for (int i = 0; i < repetitions; i++) {
            _hl.headNod = -amount;
            yield return new WaitForSeconds(duration / repetitions / 2);
            _hl.headNod = 0;
            yield return new WaitForSeconds(duration / repetitions / 2);
        }
    }

    /// <summary>
    /// Glance away to a random point.
    /// </summary>
    /// <param name="duration">Duration.</param>
    IEnumerator GlanceAway(float duration = 0.5f, bool affectHead = false) {
        Debug.Log("GlanceAway");
        _hl.SetTarget(RandomTarget(_motion.GetTarget(), 0.2f), eyesOnly: true, headOnly: affectHead, duration: duration);
        yield return new WaitForSeconds(duration);
    }

    IEnumerator Glance(GameObject target, float duration = 0.5f, bool affectHead = false) {
        //Debug.Log("GlanceAway");
        //CustomActor actor = target.GetComponent<CustomActor>();
        Vector3 position = target.transform.position; ;
        /*if (actor != null) {
            position += new Vector3(0, actor.eyesHeight, 0);
        }*/
        _hl.SetTarget(position, eyesOnly: true, headOnly: affectHead, duration: duration);
        yield return new WaitForSeconds(duration);
    }

    /// <summary>
    /// Shake the head from side to side. Same thing goes for this as the nodding.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="repetitions">Repetitions.</param>
    /// <param name="amount">Amount.</param>
    IEnumerator ShakeHead(float duration = 0.5f, int repetitions = 2, float amount = 20) {
        Debug.Log("ShakeHead");
        for (int i = 0; i < repetitions * 2; i++) {
            amount = -amount;
            _hl.headShake = amount;
            yield return new WaitForSeconds(duration / repetitions / 2);
        }
        _hl.headShake = 0;
    }

    /// <summary>
    /// Gazes at a random point, rather than glancing.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="returnToPreviousTarget">If set to <c>true</c> return to previous target.</param>
    IEnumerator GazeRandom(float duration = 0.5f, bool returnToPreviousTarget = true) {
        Debug.Log("GazeRandom");
        Vector3 previousTarget = _motion.GetTarget();
        Vector3 target = RandomTarget(previousTarget);
        _motion.SetTarget(target);
        _hl.SetTarget(target, duration: duration);
        yield return new WaitForSeconds(duration);
        if (returnToPreviousTarget) {
            _motion.SetTarget(previousTarget);
        }
    }

    IEnumerator Gaze(GameObject target, float duration = 0.5f, bool returnToPreviousTarget = true) {
        Debug.Log("Gaze");
        Vector3 previousTarget = _motion.GetTarget();
        _motion.SetTarget(target.transform);
        if (!_motion.isLooking) {
            _motion.isLooking = true;
            yield return new WaitForSeconds(duration);
            _motion.isLooking = false;
        } else {
            yield return new WaitForSeconds(duration);
        }
        if (returnToPreviousTarget) {
            _motion.SetTarget(previousTarget);
        }
    }

    Vector3 RandomTarget(Vector3 currentTarget, float distanceFromCurrentTarget = 0.1f) {
        Vector3 target = currentTarget;
        while (Vector3.Distance(target.normalized, currentTarget.normalized) < distanceFromCurrentTarget) {
            target = Quaternion.AngleAxis(UnityEngine.Random.Range(-60, 60), Vector3.up) * Quaternion.AngleAxis(UnityEngine.Random.Range(-15, 15), Vector3.right) * (transform.position + transform.forward * 5);
        }
        return target;
    }
    #endregion
}
