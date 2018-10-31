using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[AddComponentMenu("Caldera/Game Preferences/Progress Tracker")]
public class ProgressTracker : Singleton<ProgressTracker>
{
    public enum ProgressMode
    {
        NORMAL,
        CLEAN,
        UNLOCKED,
    };
    /// <summary>
    /// 
    /// </summary>
    public GameProgress ProgressState;
    /// <summary>
    /// 
    /// </summary>
    public ProgressMode Mode = ProgressMode.NORMAL;
    /// <summary>
    /// 
    /// </summary>
    public bool DebugSaveGame = false;

    public override void Init()
    {
        if (Mode == ProgressMode.UNLOCKED)
        {
            string serializedProgress = @"{""MasterVolume"":1.0,""CurrentLanguage"":""Spanish"",""Tutorial_Completed"":true}";
            if (DebugSaveGame)
                Debug.Log("Game unlocked, loading the following save: " + serializedProgress);
            DeserializeProgress(serializedProgress);
            Verify();
        }
        else if (Mode == ProgressMode.CLEAN)
        {
            ProgressState = new GameProgress();
            ProgressState.LoadDefaultValues();
        }
        else if (Mode == ProgressMode.NORMAL && PlayerPrefs.HasKey("GameProgressString"))
        {
            string serializedProgress = PlayerPrefs.GetString("GameProgressString");
            if (DebugSaveGame)
                Debug.Log("Progress loaded from PlayerPrefs: " + serializedProgress);
            DeserializeProgress(serializedProgress);
            Verify();
        }
        else
        {
            // Normal progression but a previous save file was not found.
            ProgressState = new GameProgress();
            ProgressState.LoadDefaultValues();
        }
    }

    /// <summary>
    /// This function checks if the saved data still matches the available data. Useful when 
    /// adding or removing new levels (between builds, for example).
    /// </summary>
    public void Verify()
    {
    }

    public void SaveProgress()
    {
        StartCoroutine(SaveProgressCoroutine());
    }

    public void Logout()
    {
        StartCoroutine(LogoutCoroutine());
    }

    private IEnumerator LogoutCoroutine()
    {
        string entryId = ProgressTracker.Instance.ProgressState.entryId;
        string token = ProgressTracker.Instance.ProgressState.sessionToken;

        using (UnityWebRequest www = UnityWebRequest.Post("http://enjambre.cultura.gob.mx/api/retos/close/" + entryId + "/" + token, ""))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                //JObject entry = JObject.Parse(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.text);
            }
        }

        ProgressTracker.Instance.ProgressState.username = string.Empty;
        ProgressTracker.Instance.ProgressState.password = string.Empty;

        PlayerPrefs.SetString("GameProgressString", SerializeProgress());

        if (SceneManager.GetActiveScene().name.Equals("Main"))
            StreamLoader.Instance.Load("Login");
    }

    public IEnumerator SaveProgressCoroutine()
    {
        ProgressState.saveTime = DateTime.Now;
        string serializedProgress = SerializeProgress();

        if (DebugSaveGame)
            Debug.Log("Attempting to save the following progress: " + serializedProgress);


        PlayerPrefs.SetString("GameProgressString", serializedProgress);

        if (string.IsNullOrEmpty(ProgressState.entryId) || string.IsNullOrEmpty(ProgressState.sessionToken))
            yield break;
        
        /*WWWForm form = new WWWForm();
        form.AddField("statusStr", serializedProgress);

        using (UnityWebRequest www = UnityWebRequest.Post("http://enjambre.cultura.gob.mx/api/retos/status/" + ProgressState.entryId + "/" + ProgressState.sessionToken, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);

                ProgressTracker.Instance.ProgressState.username = string.Empty;
                ProgressTracker.Instance.ProgressState.password = string.Empty;

                PlayerPrefs.SetString("GameProgressString", SerializeProgress());

                if (SceneManager.GetActiveScene().name.Equals("Main"))
                    StreamLoader.Instance.Load("Login");
            }
            else
            {
                JObject entry = JObject.Parse(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.text);
                if (!entry["mensaje"].Value<string>().Equals("ok"))
                    Logout();
                else
                    Debug.Log(www.downloadHandler.text);
            }
        }*/
    }

    public string SerializeProgress()
    {
        return JsonConvert.SerializeObject(ProgressState);
    }

    public void ResetProgress()
    {
        ProgressState = new GameProgress();
        ProgressState.LoadDefaultValues();
    }

    public void DeserializeProgress(string progressString)
    {
        if (!string.IsNullOrEmpty(progressString))
        {
            GameProgress temp = null;
            try
            {
                temp = JsonConvert.DeserializeObject<GameProgress>(progressString);
            }
            catch (Exception e)
            {
                Debug.Log("ERROR: Something happened while trying to deserialize the progress string: " + progressString + ". "
                    + "\nUsing default values for the game progress. "
                    + "\nThe error message is: " + e.Message);
            }

            if (temp != null)
                ProgressState = temp;
        }

        if (ProgressState == null)
        {
            ProgressState = new GameProgress();
            ProgressState.LoadDefaultValues();
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveProgress();
    }

    void OnApplicationQuit()
    {
        SaveProgress();
    }
}