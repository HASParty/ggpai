using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Finds and organises bones in a skeleton.
/// Tries to be somewhat efficient.
/// </summary>
public class AgentBoneFinder : MonoBehaviour {

    List<Transform> transforms;
    public int count = 0;
    Dictionary<string, int> bone_seeker;

    public Transform findBone(string name) {
        if (bone_seeker == null) {
            bone_seeker = new Dictionary<string, int>();
            transforms = new List<Transform>();
        }
        name = name.ToLower();
        if (bone_seeker.ContainsKey(name)) {
            return transforms[bone_seeker[name]];
        }
        Transform bone = findBone(transform, name);
        return bone;
    }

    Transform findBone(Transform parent, string name) {
        Transform ret = null;

        foreach (Transform child in parent) {
            if (!bone_seeker.ContainsKey(child.name.ToLower())) {
                bone_seeker.Add(child.name.ToLower(), count);
                transforms.Add(child);
                count++;
            }
            if (child.name.ToLower() == name) {
                return child;
            }
            ret = findBone(child, name);
            if (ret != null) return ret;
        }
        return ret;
    }
}
