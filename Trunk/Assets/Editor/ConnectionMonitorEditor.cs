using UnityEngine;
using UnityEditor;
using Boardgame.Networking;

[CustomEditor(typeof(ConnectionMonitor))]
public class ConnectionMonitorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("Game connection ("+ ConnectionMonitor.Instance.Host+":"+ ConnectionMonitor.Instance.GamePort +"): " + ConnectionMonitor.Instance.GameConnectionStatus);
        EditorGUILayout.LabelField("Feed connection (" + ConnectionMonitor.Instance.Host + ":" + ConnectionMonitor.Instance.FeedPort + "): " + ConnectionMonitor.Instance.FeedConnectionStatus);   
        if (Application.isPlaying) {
            if (GUILayout.Button("(Re)connect"))
            {
                ConnectionMonitor c = target as ConnectionMonitor;
                c.Disconnect();
                c.Connect();
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
