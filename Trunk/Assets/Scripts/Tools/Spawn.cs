using UnityEngine;
using System.Collections;

/// <summary>
/// Tidy way to spawn monobehaviour scripts
/// </summary>
/// <typeparam name="T">Script type</typeparam>
public static class ScriptHelper<T> where T : MonoBehaviour {
    public static T spawn(Transform parent) {
        T obj = new GameObject().AddComponent<T>();
        obj.transform.SetParent(parent);
        return obj;
    }
}
