using UnityEngine;
using UnityEditor;
using Boardgame.Agent;
using Boardgame.Configuration;

[CustomEditor(typeof(PersonalityModule))]
public class PersonalityModuleEditor : Editor {
    SerializedProperty agreeableness, agreeablenessWeight;
    SerializedProperty conscientiousness, conscientiousnessWeight;
    SerializedProperty extraversion, extraversionWeight;
    SerializedProperty neuroticism, neuroticismWeight;
    SerializedProperty openness, opennessWeight;


    SerializedProperty arousalDecay, valenceDecay;

    PersonalityModule pm;

    bool showTraits = false;

    void OnEnable() {
        pm = target as PersonalityModule;
        pm.ReloadPersonality();
        pm.Recalc();

        agreeableness = serializedObject.FindProperty("agreeableness");
        conscientiousness = serializedObject.FindProperty("conscientiousness");
        extraversion = serializedObject.FindProperty("extraversion");
        neuroticism = serializedObject.FindProperty("neuroticism");
        openness = serializedObject.FindProperty("openness");

        arousalDecay = serializedObject.FindProperty("arousalBaseDecayRate");
        valenceDecay = serializedObject.FindProperty("valenceBaseDecayRate");
    }

    void OnSceneGUI() {
        float lo = Config.Low;
        float ne = Config.Neutral;
        float hi = Config.High;
        Transform transform = pm.transform;
        Handles.DrawLine(new Vector3(lo, ne) + transform.position, new Vector3(hi, ne) + transform.position);
        Handles.DrawLine(new Vector3(ne, lo) + transform.position, new Vector3(ne, hi) + transform.position);
        Handles.DrawWireDisc(new Vector3(ne, ne) + transform.position, Vector3.forward, (hi - lo) / 2);
        Handles.color = Color.red;
        Handles.DrawLine(new Vector3(ne, ne) + transform.position, new Vector3(pm.GetValence(), pm.GetArousal()) + transform.position);
        Handles.DrawWireDisc(new Vector3(pm.GetValence(), pm.GetArousal()) + transform.position, Vector3.forward, 0.05f);
    }

    public override void OnInspectorGUI() {
        GUIStyle bold = new GUIStyle() { fontStyle = FontStyle.Bold };
        serializedObject.Update();
        EditorGUILayout.LabelField("Personality and modifiers", bold);
        EditorGUI.indentLevel++;
        if (showTraits) {
            EditorGUILayout.LabelField("Arousal | Valence");
        }
        DisplayTrait("Agreeableness", agreeableness);
        DisplayTrait("Conscientiousness", conscientiousness);
        DisplayTrait("Extraversion", extraversion);
        DisplayTrait("Neuroticism", neuroticism);
        DisplayTrait("Openness", openness);
        if (GUILayout.Button("Reload personality config")) {
            pm.ReloadPersonality();
        }
        if (GUILayout.Button(showTraits ? "Show less" : "Show more")) {
            showTraits = !showTraits;
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Mood", bold);
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Arousal: " + pm.GetArousal(), GUILayout.Width(120));
        EditorGUILayout.LabelField("Valence: " + pm.GetValence(), GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Arousal decay / s: " + pm.getCurrentArousalDecay());
        EditorGUILayout.LabelField("Valence decay / s: " + pm.getCurrentValenceDecay());
        EditorGUILayout.PropertyField(arousalDecay);
        EditorGUILayout.PropertyField(valenceDecay);
        if (GUILayout.Button("Recalculate")) {
            pm.Recalc();
        }
        EditorGUI.indentLevel--;
        serializedObject.ApplyModifiedProperties();
    }

    private void DisplayTrait(string name, SerializedProperty trait) {
        var bold = new GUIStyle() { fontStyle = FontStyle.Bold };
        EditorGUILayout.LabelField(name + ":", bold);
        EditorGUI.indentLevel++;
        SerializedProperty value = trait.FindPropertyRelative("value");
        EditorGUILayout.LabelField("Value is " + value.intValue);
        if (showTraits) {
            EditorGUILayout.PropertyField(trait, true);
        }
        EditorGUI.indentLevel--;
    }


}
