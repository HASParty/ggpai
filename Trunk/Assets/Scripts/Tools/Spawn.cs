using UnityEngine;
using System.Collections;

public static class ScriptHelper<T> where T : MonoBehaviour {
    public static T spawn(Transform parent) {
        T obj = new GameObject().AddComponent<T>();
        obj.transform.SetParent(parent);
        return obj;
    }
}
