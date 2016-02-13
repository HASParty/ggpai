using UnityEngine;
using UnityEditor;
using Boardgame.Networking;

[CustomEditor(typeof(ConnectionMonitor))]
public class ConnectionMonitorEditor : Editor
{
    int newGPort;
    int newFPort;
    string newHost;
    string sendTest;
    bool editSettings = false;
    ConnectionMonitor c;

    public void OnEnable()
    {
        c = target as ConnectionMonitor;
        newHost = c.Host;
        newFPort = c.FeedPort;
        newGPort = c.GamePort;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("Game connection ("+ c.Host+":"+ c.GamePort +"): " + c.GameConnectionStatus);
        EditorGUILayout.LabelField("Feed connection (" + c.Host + ":" + c.FeedPort + "): " + c.FeedConnectionStatus);
        if (editSettings)
        {
            newHost = EditorGUILayout.TextField("Change hostname:", newHost);
            newGPort = EditorGUILayout.IntField("Change game port:", newGPort);
            newFPort = EditorGUILayout.IntField("Change feed port:", newFPort);
            if (newHost != c.Host || newGPort != c.GamePort || newFPort != c.FeedPort)
            {
                if (GUILayout.Button("Save changes"))
                {
                    c.UpdateSettings(newHost, newGPort, newFPort);
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
        if (GUILayout.Button((c.IsConnected() ? "Reconnect" : "Connect")))
        {
            c.Disconnect();
            c.Connect();
        }
        if(c.IsConnected())
        {
            if(GUILayout.Button("Disconnect"))
            {
                c.Disconnect();
            }

            sendTest = EditorGUILayout.TextField("Send test:", sendTest);
            if (sendTest.Length > 0)
            {
                if (GUILayout.Button("Send"))
                {
                    c.Write(sendTest);
                    sendTest = "";
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
