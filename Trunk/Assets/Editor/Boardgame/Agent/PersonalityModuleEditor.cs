using UnityEngine;
using UnityEditor;
using Boardgame.Agent;

[CustomEditor(typeof(PersonalityModule))]
public class PersonalityModuleEditor : Editor {
    SerializedProperty agreeableness, agreeablenessWeight;
    SerializedProperty conscientiousness, conscientiousnessWeight;
    SerializedProperty extraversion, extraversionWeight;
    SerializedProperty neuroticism, neuroticismWeight;
    SerializedProperty openness, opennessWeight;


    SerializedProperty arousalDecay, valenceDecay, arousalActualDecay, valenceActualDecay;

    PersonalityModule pm;

    bool showTraits = false;

    void OnEnable()
    {
        pm = target as PersonalityModule;
        pm.ReloadIdentikit();
        pm.RecalcDecayRate();
        pm.RecalcRestingMood();
        pm.ResetMood();

        agreeableness = serializedObject.FindProperty("agreeableness");
        conscientiousness = serializedObject.FindProperty("conscientiousness");
        extraversion = serializedObject.FindProperty("extraversion");
        neuroticism = serializedObject.FindProperty("neuroticism");
        openness = serializedObject.FindProperty("openness");

        arousalDecay = serializedObject.FindProperty("arousalBaseDecayRate");
        valenceDecay = serializedObject.FindProperty("valenceBaseDecayRate");

        arousalActualDecay = serializedObject.FindProperty("arousalDecayRate");
        valenceActualDecay = serializedObject.FindProperty("valenceDecayRate");
    }

    void OnSceneGUI()
    {
        float lo = PersonalityModule.Low;
        float ne = PersonalityModule.Neutral;
        float hi = PersonalityModule.High;
        Transform transform = pm.transform;
        Handles.DrawLine(new Vector3(lo, ne) + transform.position, new Vector3(hi, ne) + transform.position);
        Handles.DrawLine(new Vector3(ne, lo) + transform.position, new Vector3(ne, hi) + transform.position);
        Handles.DrawWireDisc(new Vector3(ne, ne) + transform.position, Vector3.forward, (hi-lo)/2);
        Handles.color = Color.red;
        Handles.DrawLine(new Vector3(ne, ne) + transform.position, new Vector3(pm.GetValence(), pm.GetArousal()) + transform.position);
        Handles.DrawWireDisc(new Vector3(pm.GetValence(), pm.GetArousal()) + transform.position, Vector3.forward, 0.05f);
    }

    public override void OnInspectorGUI()
    {
        GUIStyle bold = new GUIStyle() { fontStyle = FontStyle.Bold };
        serializedObject.Update();
        EditorGUILayout.LabelField("Personality and modifiers", bold);
        EditorGUI.indentLevel++;
        if (showTraits)
        {
            EditorGUILayout.LabelField("Arousal | Valence");
        }
        DisplayTrait("Agreeableness", agreeableness);
        DisplayTrait("Conscientiousness", conscientiousness);
        DisplayTrait("Extraversion", extraversion);
        DisplayTrait("Neuroticism", neuroticism);
        DisplayTrait("Openness", openness);
        if (GUILayout.Button("Reload identikit"))
        {
            pm.ReloadIdentikit();
        }
        if (GUILayout.Button(showTraits ? "Show less" : "Show more"))
        {
            showTraits = !showTraits;
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Mood", bold);
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Arousal: " + pm.GetArousal(), GUILayout.Width(120));
        EditorGUILayout.LabelField("Valence: " + pm.GetValence(), GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Arousal decay / s: " + arousalActualDecay.floatValue);
        EditorGUILayout.LabelField("Valence decay / s: " + valenceActualDecay.floatValue);
        EditorGUILayout.PropertyField(arousalDecay);
        EditorGUILayout.PropertyField(valenceDecay);
        if(GUILayout.Button("Recalculate"))
        {
            pm.RecalcDecayRate();
            pm.RecalcRestingMood();
            pm.ResetMood();
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Resulting data", bold);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("emotion = " + pm.GetEmotion().ToString().ToLower().Replace('_',' ') + ", intensity = "+ pm.GetArousal());
        serializedObject.ApplyModifiedProperties();
    }

    private void DisplayTrait(string name, SerializedProperty trait)
    {
        var bold = new GUIStyle() { fontStyle = FontStyle.Bold };
        EditorGUILayout.LabelField(name + ":", bold);
        EditorGUI.indentLevel++;
        SerializedProperty value = trait.FindPropertyRelative("value");
        EditorGUILayout.LabelField("Value is " + value.intValue);
        if (showTraits)
        {
            SerializedProperty v = trait.FindPropertyRelative("valenceEffectHigh");
            SerializedProperty a = trait.FindPropertyRelative("arousalEffectHigh");
            EditorGUILayout.LabelField("High mood effect (weight)");
            EditorGUILayout.BeginHorizontal();
            a.floatValue = EditorGUILayout.FloatField(a.floatValue);
            v.floatValue = EditorGUILayout.FloatField(v.floatValue);
            EditorGUILayout.EndHorizontal();
            v = trait.FindPropertyRelative("valenceEffectLow");
            a = trait.FindPropertyRelative("arousalEffectLow");
            EditorGUILayout.LabelField("Low mood effect (weight)");
            EditorGUILayout.BeginHorizontal();
            a.floatValue = EditorGUILayout.FloatField(a.floatValue);
            v.floatValue = EditorGUILayout.FloatField(v.floatValue);
            EditorGUILayout.EndHorizontal();
            v = trait.FindPropertyRelative("valenceInterpolation");
            a = trait.FindPropertyRelative("arousalInterpolation");
            EditorGUILayout.LabelField("Interpolation effect (adds/subs)");
            EditorGUILayout.BeginHorizontal();
            a.floatValue = EditorGUILayout.FloatField(a.floatValue);
            v.floatValue = EditorGUILayout.FloatField(v.floatValue);
            EditorGUILayout.EndHorizontal();
            v = trait.FindPropertyRelative("valenceDecay");
            a = trait.FindPropertyRelative("arousalDecay");
            EditorGUILayout.LabelField("Decay effect (adds/subs)");
            EditorGUILayout.BeginHorizontal();
            a.floatValue = EditorGUILayout.FloatField(a.floatValue);
            v.floatValue = EditorGUILayout.FloatField(v.floatValue);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
    }


}
