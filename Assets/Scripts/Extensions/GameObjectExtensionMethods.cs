using UnityEngine;
using System.Collections;

public static class GameObjectExtensionMethods
{
    public static string GetFullPath(this GameObject go)
    {
        Transform transform = go.transform;
        
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }
}