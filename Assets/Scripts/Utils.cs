using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    // toggle the active-state of the given game object
    public static void ToggleGameObject(GameObject go)
    {
        go.SetActive(!go.activeInHierarchy);
    }

    // return the given vector, but with the x-value set to 0
    public static Vector3 WithoutX(Vector3 vector)
    {
        vector.x = 0;
        return vector;
    }

    // return the given vector, but with the y-value set to 0
    public static Vector3 WithoutY(Vector3 vector)
    {
        vector.y = 0;
        return vector;
    }

    // return the given vector, while overriding all values, for which the parameter is not null
    public static Vector3 VectorOverride(Vector3 vector, float? x, float? y, float? z)
    {
        if (x.HasValue) vector.x = x.Value;
        if (y.HasValue) vector.y = y.Value;
        if (z.HasValue) vector.z = z.Value;

        return vector;
    }

    // recursively collect all bones contained below the given bone transform, including the bone itself
    public static List<Transform> CollectBoneTransforms(Transform bone)
    {
        List<Transform> bones = new List<Transform>() { bone };
        if (bone.childCount == 0) return bones;

        foreach (Transform child in bone)
        {
            bones.AddRange(CollectBoneTransforms(child));
        }

        return bones;
    }
}
