using UnityEngine;
using System.Collections.Generic;
using Boardgame.Configuration;

public class ExpressionNode {
    public string Name;
    public Dictionary<string, Vector3> StartPos;
    public Dictionary<string, Vector3> GoalPos;
    public float Weight;
    public float TransitionTime;
    public float RevertTime;
    public float TimeElapsed;
    public float HoldTime;
    public float EaseIn;
    public float EaseOut;
    public bool RevertToNeutral = true;
    //need the sender to be able to cancel the expression

}

public class FaceControllerII : MonoBehaviour {
    public bool TriggerExpression = false;
    public string ExpressionName;
#if UNITY_EDITOR
    public string[] CurrentExpressions;
#endif
    public bool atOrigin = true;
    private Dictionary<string, Transform> bones;
    private SortedList<float, ExpressionNode> queue;
    private Dictionary<string, Vector3> boneOrigin;
    private Dictionary<string, Vector3> boneFinal;
    private Dictionary<string, List<Vector3>> boneBlender;
    private List<ExpressionNode> currentExpressions;
    private ExpressionNode currentEmotionExpression;
    private float timeElapsed;

    //blinking
    public float EyeClose = 0f;
    public float EyeDroop = 0f;

    Transform UpperLidL;
    Transform UpperLidR;
    Transform LowerLidL;
    Transform LowerLidR;
    Transform Eye;

    Vector3 UpperLidLOrigin;
    Vector3 UpperLidROrigin;
    Vector3 LowerLidLOrigin;
    Vector3 LowerLidROrigin;

    #region IRegistrable implementation

    public void Awake() {
        AgentBoneFinder abf = GetComponent<AgentBoneFinder>();
        bones = new Dictionary<string, Transform>();
        timeElapsed = 0f;
        currentExpressions = new List<ExpressionNode>();
        queue = new SortedList<float, ExpressionNode>();
        boneOrigin = new Dictionary<string, Vector3>();
        boneBlender = new Dictionary<string, List<Vector3>>();
        findFaceBones(abf.findBone("head"));
        UpperLidL = abf.findBone("UpperLidL");
        UpperLidLOrigin = UpperLidL.localPosition;
        UpperLidR = abf.findBone("UpperLidR");
        UpperLidROrigin = UpperLidR.localPosition;
        LowerLidL = abf.findBone("LowerLidL");
        LowerLidLOrigin = LowerLidL.localPosition;
        LowerLidR = abf.findBone("LowerLidR");
        LowerLidROrigin = LowerLidR.localPosition;
        Eye = abf.findBone("LeftEye");
        foreach (Transform bone in bones.Values) {
            boneOrigin.Add(bone.name, bone.localPosition);
        }
    }

    #endregion

    void findFaceBones(Transform parent) {
        foreach (Transform child in parent) {
            if (child.name == "HeadEnd" || child.name.Contains("Eye") || child.name.Contains("Lid")) continue;
            bones.Add(child.name, child);
            findFaceBones(child);
        }
    }

    public Dictionary<string, Transform> GetBones() {
        return bones;
    }

    Dictionary<string, Vector3> GetCurrentPos() {
        var d = new Dictionary<string, Vector3>();
        foreach (var bone in bones.Values) {
            d.Add(bone.name, bone.localPosition);
        }
        return d;
    }

    #region blinking / eyelid control
    public float BlinkIntervalMin = 2f;
    public float BlinkIntervalMax = 7f;
    public float BlinkDuration = 0.2f;
    float BlinkGoal = 1f;
    float BlinkStart = 0f;
    float BlinkElapsed = 0f;
    float BlinkTime = 0f;
    bool Blinking = false;
    bool BlinkOpen = false;
    public float blinkDistance = 0.009f;
    void Blink() {
        BlinkElapsed += Time.deltaTime;
        if (BlinkTime == 0f && !Blinking) {
            BlinkTime = Random.Range(BlinkIntervalMin, BlinkIntervalMax);
        }
        if (BlinkElapsed > BlinkTime) {
            if (!Blinking) {
                Blinking = true;
                BlinkStart = EyeClose;
                BlinkGoal = 1f;
                BlinkElapsed = 0f;
                BlinkTime = BlinkDuration / 2;
            } else if (!BlinkOpen) {
                BlinkGoal = BlinkStart;
                BlinkStart = 1f;
                BlinkElapsed = 0f;
                BlinkOpen = true;
            } else {
                Blinking = false;
                BlinkElapsed = 0f;
                BlinkTime = 0f;
                BlinkOpen = false;
            }
        }
        if (Blinking) {
            EyeClose = Mathf.Lerp(BlinkStart, BlinkGoal, BlinkElapsed / BlinkTime);
        } else {
            EyeClose = CalculateEyeClose();
        }

        UpperLidL.localPosition = new Vector3(UpperLidLOrigin.x + blinkDistance * EyeClose + EyeDroop * 0.001f, UpperLidL.localPosition.y, UpperLidL.localPosition.z);
        UpperLidR.localPosition = new Vector3(UpperLidROrigin.x + blinkDistance * EyeClose + EyeDroop * 0.001f, UpperLidR.localPosition.y, UpperLidR.localPosition.z);
        if (!Blinking) {
            LowerLidL.localPosition = new Vector3(LowerLidLOrigin.x + blinkDistance * EyeClose - EyeDroop * 0.001f, LowerLidL.localPosition.y, LowerLidL.localPosition.z);
            LowerLidR.localPosition = new Vector3(LowerLidROrigin.x + blinkDistance * EyeClose - EyeDroop * 0.001f, LowerLidR.localPosition.y, LowerLidR.localPosition.z);
        }
    }

    float CalculateEyeClose() {
        float angle = 270 - Eye.localEulerAngles.y;
        if (angle <= 5f)
            return 0f;
        return Mathf.Clamp((angle - 5f) / 30f, 0, 1f);
    }
    #endregion

    public void ScheduleExpression(string name, float transitionTime = 1f, float weight = 1f) {
        ScheduleExpression(GenerateExpressionNode(name, transitionTime: transitionTime, weight: weight));
    }

    public void ScheduleExpression(ExpressionNode expr, float time = -1f) {
        if (expr == null) return;
        if (time == -1f) time = timeElapsed;
        try { queue.Add(time, expr); } catch { }
    }

    public void ScheduleSequence(string[] expressions) {
        float time = timeElapsed;
        foreach (string s in expressions) {
            ScheduleExpression(GenerateExpressionNode(s, transitionTime: 1f, revertTime: 0.5f, holdTime: 0.2f), time);
            time += 2f;
        }
    }

    public float ScheduleFlapperString(List<KeyValuePair<string, float[]>> phonemes) {
        float time = timeElapsed;
        foreach (var phoneme in phonemes) {
            if (ExpressionLibrary.Contains(phoneme.Key.ToString())) {
                var offset = GenerateExpressionNode(phoneme.Key, phoneme.Value[0], phoneme.Value[1]);
                ScheduleExpression(offset, time);
                time += offset.TransitionTime + offset.HoldTime;
            }
        }
        return time - timeElapsed;
    }

    public void ScheduleVisemes(List<KeyValuePair<string, float[]>> visemes) {
        foreach (var viseme in visemes) {
            //Debug.Log ("Scheduling viseme "+viseme.Key+" in "+viseme.Value[0]+" for "+viseme.Value[1]);
            if (ExpressionLibrary.Contains(viseme.Key.ToString())) {
                var offset = GenerateExpressionNode(viseme.Key, viseme.Value[1] / 2, viseme.Value[1] / 2, viseme.Value[1] / 2);
                offset.RevertToNeutral = false;
                ScheduleExpression(offset, timeElapsed + viseme.Value[0]);
            } else {
                Debug.Log("Viseme did not exist.");
            }
        }
    }

    void RevertExpression(ExpressionNode revertFrom = null) {
        var offset = new ExpressionNode();
        offset.Name = "neutral";
        offset.TransitionTime = 0.2f;
        offset.HoldTime = 0f;
        offset.EaseIn = 0f;
        offset.EaseOut = 0f;
        offset.Weight = 1f;
        var goal = new Dictionary<string, Vector3>();
        if (revertFrom == null) {
            foreach (var bone in bones.Keys) {
                goal.Add(bone, Vector3.zero);
            }
        } else {
            offset.TransitionTime = revertFrom.RevertTime;
            var start = new Dictionary<string, Vector3>();
            foreach (var bone in revertFrom.GoalPos) {
                goal.Add(bone.Key, Vector3.zero);
                start.Add(bone.Key, bone.Value + boneOrigin[bone.Key]);
            }
            offset.StartPos = start;
        }
        offset.GoalPos = goal;
        ScheduleExpression(offset, timeElapsed);
    }

    public bool ShowingExpression(string expression) {
        foreach (ExpressionNode e in currentExpressions) {
            if (e.Name == expression) return true;
        }
        return false;
    }

    public static ExpressionNode GenerateExpressionNode(string expression, float transitionTime = 1f, float revertTime = 1f, float holdTime = 0f, float easeIn = 0f, float easeOut = 0f, float weight = 1f, float modifier = 1f) {
        var offset = new ExpressionNode();
        offset.Name = expression;
        offset.TransitionTime = transitionTime;
        offset.RevertTime = revertTime;
        offset.HoldTime = holdTime;
        offset.EaseIn = easeIn;
        offset.EaseOut = easeOut;
        offset.Weight = weight;
        if (ExpressionLibrary.Contains(expression)) {
            offset.GoalPos = new Dictionary<string, Vector3>(ExpressionLibrary.Get(expression));
            var keys = new List<string>(offset.GoalPos.Keys);
            for (int i = 0; i < keys.Count; i++) {
                offset.GoalPos[keys[i]] = Vector3.Scale(offset.GoalPos[keys[i]], new Vector3(modifier, modifier, modifier));
            }
        } else {
            return null;
        }
        return offset;
    }

    public static ExpressionNode GenerateEmotionalExpression(float arousal, float valence, float transitionTime = 1f) {
        var offset = new ExpressionNode();
        offset.Name = string.Format("{0} {1}", arousal, valence);
        offset.TransitionTime = transitionTime;
        offset.RevertTime = -1f;
        offset.HoldTime = 0f;
        offset.EaseIn = 0;
        offset.EaseOut = 0;
        offset.Weight = 0.7f;

        string valenceExpr = (valence < Config.Neutral ? "unhappy" : "happy");
        string arousalExpr = (arousal < Config.Neutral ? "calm" : "surprised");
        float v = Mathf.Abs(valence - Config.Neutral);
        float a = Mathf.Abs(arousal - Config.Neutral);
        Vector3 valenceWeight = new Vector3(v, v, v);
        Vector3 arousalWeight = new Vector3(a, a, a);
        offset.GoalPos = new Dictionary<string, Vector3>();

        if (ExpressionLibrary.Contains(valenceExpr)) {
            var valGoal = new Dictionary<string, Vector3>(ExpressionLibrary.Get(valenceExpr));
            var aroGoal = new Dictionary<string, Vector3>(ExpressionLibrary.Get(arousalExpr));
            float u = 0.8f;
            float l = -0.02f;
            foreach(var key in valGoal.Keys)
            {
                Vector3 tinyError = new Vector3(Random.Range(l, u), Random.Range(l, u), Random.Range(l, u));
                tinyError.Scale(valenceWeight);
                var vec = Vector3.Scale(valGoal[key], valenceWeight);
                if (offset.GoalPos.ContainsKey(key)) offset.GoalPos[key] = vec + Vector3.Scale(vec, tinyError);
                else offset.GoalPos.Add(key, vec + Vector3.Scale(valGoal[key], tinyError));
            }
            foreach (var key in aroGoal.Keys)
            {
                Vector3 tinyError = new Vector3(Random.Range(l, u), Random.Range(l, u), Random.Range(l, u));
                tinyError.Scale(arousalWeight);
                var vec = Vector3.Scale(aroGoal[key], arousalWeight);
                if (offset.GoalPos.ContainsKey(key)) offset.GoalPos[key] += vec + Vector3.Scale(vec, tinyError);
                else offset.GoalPos.Add(key, vec + Vector3.Scale(aroGoal[key], tinyError));
            }
            
        } else {
            throw new UnityException("what the fuck");
        }

        return offset;
    }
    void LerpFace() {
#if UNITY_EDITOR
        CurrentExpressions = new string[currentExpressions.Count];
#endif
        if (currentExpressions.Count > 0) {

            boneBlender.Clear();
            List<ExpressionNode> flaggedForDeletion = new List<ExpressionNode>();
            for (int i = 0; i < currentExpressions.Count; i++) {
#if UNITY_EDITOR
                CurrentExpressions[i] = currentExpressions[i].Name;
#endif
                currentExpressions[i].TimeElapsed += Time.deltaTime;
                ExpressionNode ex = currentExpressions[i];
                if (ex.Name != "neutral") atOrigin = false;
                if (ex.StartPos == null) ex.StartPos = GetCurrentPos();
                foreach (var b in ex.GoalPos) {
                    if (!boneBlender.ContainsKey(b.Key)) {
                        boneBlender.Add(b.Key, new List<Vector3>());
                    }
                    if (ex.TimeElapsed >= ex.TransitionTime && (ex.HoldTime + ex.TransitionTime) >= ex.TimeElapsed) {
                        boneBlender[b.Key].Add(boneOrigin[b.Key] + (b.Value * ex.Weight));
                    } else if (ex.TimeElapsed < ex.TransitionTime) {
                        boneBlender[b.Key].Add(Vector3.Lerp(ex.StartPos[b.Key], boneOrigin[b.Key] + b.Value * ex.Weight, ex.TimeElapsed / ex.TransitionTime));
                    } else if (ex.RevertTime == -1f) {
                        if (currentEmotionExpression.Equals(ex)) {
                            boneBlender[b.Key].Add(boneOrigin[b.Key] + (b.Value * ex.Weight));
                        } else {
                            flaggedForDeletion.Add(ex);
                        }
                    } else {
                        flaggedForDeletion.Add(ex);
                    }
                }
            }

            //additive
            foreach (var bone in boneBlender) {
                 if (bone.Value.Count > 0) {
                     float div = 1f / bone.Value.Count;
                     Vector3 goalPos = Vector3.zero;
                     foreach (var loc in bone.Value) {
                         goalPos += loc * div;
                     }
                     bones[bone.Key].localPosition = goalPos;
                 }
             }

            foreach (var ex in flaggedForDeletion) {
                for (int i = 0; i < currentExpressions.Count; i++) {
                    var cur = currentExpressions[i];
                    if (ex.Name == cur.Name) continue;
                    if (cur.TimeElapsed < cur.TransitionTime) {
                        cur.TimeElapsed = 0;
                        cur.TransitionTime = cur.TransitionTime - cur.TimeElapsed;
                        cur.StartPos = GetCurrentPos();
                    }
                }
                //Debug.Log ("ended"+ex.Name);
                currentExpressions.Remove(ex);
                if (ex.Name != "neutral" && ex.RevertToNeutral) RevertExpression(ex);
            }
        } else {
            RevertExpression();
        }
    }

    // Update is called once per frame
    void LateUpdate() {
        timeElapsed += Time.deltaTime;
        if (queue == null) {
            Debug.LogWarning("Queue went null again");
            queue = new SortedList<float, ExpressionNode>();
        }
        while (queue.Count > 0 && queue.Keys[0] <= timeElapsed) {
            if (queue.Values[0].RevertTime == -1f) currentEmotionExpression = queue.Values[0];
            currentExpressions.Add(queue.Values[0]);
            queue.RemoveAt(0);
        }

        if (TriggerExpression) {
            if (ExpressionName.Contains(",")) {
                ScheduleSequence(ExpressionName.Split(','));
            } else {
                ScheduleExpression(GenerateExpressionNode(ExpressionName, transitionTime: 0.2f, revertTime: 0.2f, holdTime: 1f), timeElapsed);
            }
            TriggerExpression = false;
        }

        Blink();
        //lerp/derp the face
        LerpFace();

    }
}
