using UnityEngine;
using System.Collections;

public class KeepGameObjectAlive : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}