using UnityEngine;
using UnityEditor;
using Boardgame.Networking;

[CustomEditor(typeof(ConnectionMonitor))]
public class ConnectionMonitorEditor : Editor
{
    int newGPort;
    int newFPort;
    string newHost;
    bool editSettings = false;

    public void OnEnable()
    {
        newHost = ConnectionMonitor.Instance.Host;
        newFPort = ConnectionMonitor.Instance.FeedPort;
        newGPort = ConnectionMonitor.Instance.GamePort;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("Game connection ("+ ConnectionMonitor.Instance.Host+":"+ ConnectionMonitor.Instance.GamePort +"): " + ConnectionMonitor.Instance.GameConnectionStatus);
        EditorGUILayout.LabelField("Feed connection (" + ConnectionMonitor.Instance.Host + ":" + ConnectionMonitor.Instance.FeedPort + "): " + ConnectionMonitor.Instance.FeedConnectionStatus);
        if (editSettings)
        {
            newHost = EditorGUILayout.TextField("Change hostname:", newHost);
            newGPort = EditorGUILayout.IntField("Change game port:", newGPort);
            newFPort = EditorGUILayout.IntField("Change feed port:", newFPort);
            if (newHost != ConnectionMonitor.Instance.Host || newGPort != ConnectionMonitor.Instance.GamePort || newFPort != ConnectionMonitor.Instance.FeedPort)
            {
                if (GUILayout.Button("Save changes"))
                {
                    ConnectionMonitor.Instance.UpdateSettings(newHost, newGPort, newFPort);
                }
            } else
            {
                if (GUILayout.Button("Hide settings"))
                {
                    editSettings = false;
                }
            }
        } else
        {
            if(GUILayout.Button("Edit settings"))
            {
                editSettings = true;
            }
        }
        if (GUILayout.Button((ConnectionMonitor.Instance.IsConnected() ? "Reconnect" : "Connect")))
        {
            ConnectionMonitor c = target as ConnectionMonitor;
            c.Disconnect();
            c.Connect();
        }
        if(ConnectionMonitor.Instance.IsConnected())
        {
            if(GUILayout.Button("Disconnect"))
            {
                ConnectionMonitor c = target as ConnectionMonitor;
                c.Disconnect();
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
