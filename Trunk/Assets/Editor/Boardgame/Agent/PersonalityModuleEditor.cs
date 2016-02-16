using UnityEngine;
using UnityEditor;
using Boardgame.Agent;

[CustomEditor(typeof(PersonalityModule))]
public class PersonalityModuleEditor : Editor {
    SerializedProperty agreeableness, agreeablenessModifier;
    SerializedProperty conscientiousness, conscientiousnessModifier;
    SerializedProperty extraversion, extraversionModifier;
    SerializedProperty neuroticism, neuroticismModifier;
    SerializedProperty openness, opennessModifier;

    PersonalityModule pm;

    void OnEnable()
    {
        pm = target as PersonalityModule;
        pm.ReloadIdentikit();


        agreeableness = serializedObject.FindProperty("agreeableness");
        agreeablenessModifier = serializedObject.FindProperty("agreeablenessModifier");
        conscientiousness = serializedObject.FindProperty("conscientiousness");
        conscientiousnessModifier = serializedObject.FindProperty("conscientiousnessModifier");
        extraversion = serializedObject.FindProperty("extraversion");
        extraversionModifier = serializedObject.FindProperty("extraversionModifier");
        neuroticism = serializedObject.FindProperty("neuroticism");
        neuroticismModifier = serializedObject.FindProperty("neuroticismModifier");
        openness = serializedObject.FindProperty("openness");
        opennessModifier = serializedObject.FindProperty("opennessModifier");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("Personality values and modifiers", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.PrefixLabel("Agreeableness: " + agreeableness.floatValue);
        agreeablenessModifier.floatValue = EditorGUILayout.FloatField((agreeablenessModifier.floatValue));
        EditorGUILayout.LabelField("Conscientiousness: " + conscientiousness.floatValue);
        EditorGUILayout.LabelField("Extraversion: " + extraversion.floatValue);
        EditorGUILayout.LabelField("Neuroticism: " + neuroticism.floatValue);
        EditorGUILayout.LabelField("Openness: " + openness.floatValue);
        serializedObject.ApplyModifiedProperties();

    }


}
