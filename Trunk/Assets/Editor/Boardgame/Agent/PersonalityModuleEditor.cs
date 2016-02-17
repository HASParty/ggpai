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

    SerializedProperty low, neutral, high;

    SerializedProperty arousalDecay, valenceDecay, arousalMod, valenceMod;

    PersonalityModule pm;

    void OnEnable()
    {
        pm = target as PersonalityModule;
        pm.ReloadIdentikit();

        low = serializedObject.FindProperty("lowVal");
        neutral = serializedObject.FindProperty("neutralVal");
        high = serializedObject.FindProperty("highVal");

        agreeableness = serializedObject.FindProperty("agreeableness");
        agreeablenessWeight = serializedObject.FindProperty("agreeablenessWeight");
        conscientiousness = serializedObject.FindProperty("conscientiousness");
        conscientiousnessWeight = serializedObject.FindProperty("conscientiousnessWeight");
        extraversion = serializedObject.FindProperty("extraversion");
        extraversionWeight = serializedObject.FindProperty("extraversionWeight");
        neuroticism = serializedObject.FindProperty("neuroticism");
        neuroticismWeight = serializedObject.FindProperty("neuroticismWeight");
        openness = serializedObject.FindProperty("openness");
        opennessWeight = serializedObject.FindProperty("opennessWeight");

        arousalDecay = serializedObject.FindProperty("arousalDecayRate");
        valenceDecay = serializedObject.FindProperty("valenceDecayRate");

        arousalMod = serializedObject.FindProperty("arousalIntensityWeight");
        valenceMod = serializedObject.FindProperty("valenceIntensityWeight");
    }

    public override void OnInspectorGUI()
    {
        GUIStyle bold = new GUIStyle() { fontStyle = FontStyle.Bold };
        serializedObject.Update();
        EditorGUILayout.LabelField("Low/Neutral/High values", bold);
        EditorGUILayout.BeginHorizontal();
        low.floatValue = EditorGUILayout.FloatField(low.floatValue);
        neutral.floatValue = EditorGUILayout.FloatField(neutral.floatValue);
        high.floatValue = EditorGUILayout.FloatField(high.floatValue);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Personality and modifiers", bold);
        EditorGUI.indentLevel = 1;
        DisplayTrait("Agreeableness", agreeableness, agreeablenessWeight);
        DisplayTrait("Conscientiousness", conscientiousness, conscientiousnessWeight);
        DisplayTrait("Extraversion", extraversion, extraversionWeight);
        DisplayTrait("Neuroticism", neuroticism, neuroticismWeight);
        DisplayTrait("Openness", openness, opennessWeight);
        if (GUILayout.Button("Reload identikit"))
        {
            pm.ReloadIdentikit();
        }
        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("Mood", bold);
        EditorGUI.indentLevel = 1;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Arousal: " + pm.GetArousal(), GUILayout.Width(120));
        EditorGUILayout.LabelField("Valence: " + pm.GetValence(), GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(arousalDecay);
        EditorGUILayout.PropertyField(valenceDecay);
        EditorGUILayout.PropertyField(arousalMod);
        EditorGUILayout.PropertyField(valenceMod);
        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("Resulting data", bold);
        EditorGUI.indentLevel = 1;
        EditorGUILayout.LabelField("Currently feeling " + pm.GetEmotion().ToString().ToLower().Replace('_',' '));
        EditorGUILayout.LabelField("At the intensity of " + pm.GetIntensity());
        serializedObject.ApplyModifiedProperties();

    }

    private void DisplayTrait(string name, SerializedProperty trait, SerializedProperty weight)
    {
        EditorGUILayout.LabelField(name + ":", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Value:", GUILayout.Width(64));
        EditorGUILayout.LabelField(trait.floatValue.ToString(), GUILayout.Width(40));
        EditorGUILayout.LabelField("Weight:", GUILayout.Width(64));
        weight.floatValue = EditorGUILayout.FloatField(weight.floatValue, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();
    }


}
