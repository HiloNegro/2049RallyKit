using UnityEngine;
using UnityEditor;
using System.Collections;

public class DeletePlayerPRefsEditor
{
    [MenuItem("Tools/Delete PlayerPrefs")]
    public static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}