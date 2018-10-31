using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

using Newtonsoft.Json.Linq;

public class LogoutButton : Selectable, IPointerClickHandler
{
    public bool deleteProgress = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        Logout();
    }

    public void Logout()
    {
        interactable = false;
        EventSystem.current.gameObject.SetActive(false);

        StartCoroutine(LogoutCoroutine());
    }

    private IEnumerator LogoutCoroutine()
    {
        string entryId = ProgressTracker.Instance.ProgressState.entryId;
        string token = ProgressTracker.Instance.ProgressState.sessionToken;

        if (deleteProgress)
        {
            ProgressTracker.Instance.ProgressState = new GameProgress();
            ProgressTracker.Instance.ProgressState.LoadDefaultValues();
            ProgressTracker.Instance.ProgressState.entryId = entryId;
            ProgressTracker.Instance.ProgressState.sessionToken = token;
            yield return StartCoroutine(ProgressTracker.Instance.SaveProgressCoroutine());
        }

        ProgressTracker.Instance.ProgressState.username = string.Empty;
        ProgressTracker.Instance.ProgressState.password = string.Empty;

        ProgressTracker.Instance.SaveProgress();

        StreamLoader.Instance.Load("Login");

        /*using (UnityWebRequest www = UnityWebRequest.Post("http://enjambre.cultura.gob.mx/api/retos/close/" + entryId + "/" + token, ""))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);

                interactable = true;
                EventSystem.current.gameObject.SetActive(true);
            }
            else
            {
                JObject entry = JObject.Parse(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.text);

                if (entry["mensaje"].Value<string>().Equals("ok close"))
                {
                    ProgressTracker.Instance.ProgressState.username = string.Empty;
                    ProgressTracker.Instance.ProgressState.password = string.Empty;

                    ProgressTracker.Instance.SaveProgress();

                    StreamLoader.Instance.Load("Login");

                    yield break;
                }
            }
        }*/
    }
}