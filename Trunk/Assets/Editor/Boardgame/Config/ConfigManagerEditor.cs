using Boardgame.Configuration;
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConfigManager))]
public class ConfigManagerEditor : Editor {
    ConfigManager cm;

    private enum Val {
        Neutral,
        Low,
        High
    }

    Val a, c, e, n, o;
    
    void OnEnable() {
        cm = target as ConfigManager;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        DrawDefaultInspector();
        a = GetValue(cm.Agreeableness);
        c = GetValue(cm.Extraversion);
        e = GetValue(cm.Conscientiousness);
        n = GetValue(cm.Neuroticism);
        o = GetValue(cm.Openness);

        a = (Val) EditorGUILayout.EnumPopup("Agreeableness", a);
        c = (Val)EditorGUILayout.EnumPopup("Conscientiousness", c);
        e = (Val)EditorGUILayout.EnumPopup("Extraversion", e);
        n = (Val)EditorGUILayout.EnumPopup("Neuroticisim", n);
        o = (Val)EditorGUILayout.EnumPopup("Openness", o);

        cm.Agreeableness = GetValue(a);
        cm.Conscientiousness = GetValue(c);
        cm.Extraversion = GetValue(e);
        cm.Neuroticism = GetValue(n);
        cm.Openness = GetValue(o);

        if (GUILayout.Button("Update config")) cm.SetConfig();
        serializedObject.ApplyModifiedProperties();
    }

    private Val GetValue(int num) {
        if(num == cm.Low) return Val.Low;
        if (num == cm.High) return Val.High;
        return Val.Neutral;
    }

    private int GetValue(Val val) {
        switch(val) {
            case Val.Low:
                return cm.Low;
            case Val.Neutral:
                return cm.Neutral;
            case Val.High:
                return cm.High;
        }
        return -1;
    }

}
