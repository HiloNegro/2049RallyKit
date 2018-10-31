using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ListExtensionMethods
{
    public static void Shuffle<T>(this IList<T> list)
    {
        //System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            //int k = rng.Next(n + 1);
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static bool HasAtLeastOneValidElement<T>(this IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null) return true;
        }

        return false;
    }
}