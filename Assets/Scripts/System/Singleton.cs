using UnityEngine;

/// <summary>
/// Base class that heirs the singleton design to any MonoBehavior
/// Manager scripts should inherit from singleton.
/// This script should not be used directly on any GameObject.
/// </summary>

[AddComponentMenu("")]
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// Instance of this singleton
    /// </summary>
    protected static T instance;

    /// <summary>
    /// Returns the instance of this singleton
    /// </summary>
    public static T Instance
    {
        // The instance is required
        get
        {
            // If there is no instance associated
            if (instance == null)
            {
                // Try to find it in the scene
                instance = (T)FindObjectOfType(typeof(T));

                // If there is no instance in the scene
                if (instance == null)
                {
                    // Create a temporary GameObject and attach this script
                    instance = new GameObject("Instance of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();
                }
            }

            // Return the instance
            return instance;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsInstanceAlive()
    {
        return instance ? true : false;
    }

    /// <summary>
    /// If no other MonoBehavior requires the instance in an awake function executing before this one,
    /// there is no need to search the object in the scene (lookup in getter).
    /// If there is already an instance in the scene, remove this script from the GameObject
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            Init();
        }
        else
        {
            Debug.Log("There is already another instance of " + typeof(T).ToString() + ", removing this script.");
            Destroy(this);
        }
    }

    /// <summary>
    /// This function is called when the instance is used for the first time.
    /// Put all the initializations you need here, as you would do in Awake.
    /// </summary>
    public virtual void Init()
    {

    }

    /// <summary>
    /// Make sure the instance isn't referenced anymore when the user exits.
    /// </summary>
    private void OnApplicationQuit()
    {
        instance = null;
    }
}