using UnityEngine;
using UnityEditor;
using Boardgame.Agent;
using Boardgame.Configuration;

[CustomEditor(typeof(PersonalityModule))]
public class PersonalityModuleEditor : Editor {
    SerializedProperty agreeableness;
    SerializedProperty conscientiousness;
    SerializedProperty extraversion;
    SerializedProperty neuroticism;
    SerializedProperty openness;

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
    }

    public override void OnInspectorGUI() {
        GUIStyle bold = new GUIStyle() { fontStyle = FontStyle.Bold };
        serializedObject.Update();
        EditorGUILayout.LabelField("Personality and modifiers", bold);
        EditorGUI.indentLevel++;
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
        if (GUILayout.Button("Recalculate")) {
            pm.Recalc();
        }
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
