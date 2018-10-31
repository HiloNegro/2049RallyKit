using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[AddComponentMenu("Caldera/Game Managers/Stream Loader")]
public class StreamLoader : Singleton<StreamLoader>
{
    /// <summary>
    /// 
    /// </summary>
    //public LoadingScreen LoadingScreenManager;

    /// <summary>
    /// 
    /// </summary>
    //public UIProgressBar ProgressBar;
    /// <summary>
    /// 
    /// </summary>
    private bool _isLoading = false;

    void Start()
    {
        _isLoading = false;

        //if (ProgressBar != null)
        //    ProgressBar.value = 0.0f;
    }

    public void ShowLoadingScreen(bool show, bool progressBar = true)
    {
        //if (!LoadingScreenManager) return;

        //LoadingScreenManager.Show(show);
        //LoadingScreenManager.ShowProgressBar(progressBar);
    }

    public void Load(string levelToLoad, bool autoHideLoadingScreen = true)
    {
        if (!string.IsNullOrEmpty(levelToLoad) && !_isLoading)
            StartCoroutine(LoadLevelAsync(levelToLoad, autoHideLoadingScreen));
    }

    //IEnumerator DownloadLevelAsync(string levelName)
    //{
    //    float downloadProgress = Application.GetStreamProgressForLevel(levelName);

    //    // Show the loading screen only when downloading data in streamed builds
    //    if(!Mathf.Approximately(downloadProgress, 1.0f) && LoadingScreenManager)
    //    {
    //        LoadingScreenManager.Show();

    //        yield return new WaitForEndOfFrame();
    //        yield return new WaitForEndOfFrame();

    //        while (LoadingScreenManager.IsAnimating())
    //            yield return null;
    //    }

    //    while (!Mathf.Approximately(downloadProgress, 1.0f))
    //    {
    //        if (ProgressBar != null)
    //            ProgressBar.value = downloadProgress;

    //        downloadProgress = Application.GetStreamProgressForLevel(levelName);

    //        yield return null;
    //    }

    //    if (ProgressBar != null)
    //        ProgressBar.value = 1.0f;

    //    while (!Application.CanStreamedLevelBeLoaded(levelName))
    //    {
    //        yield return null;
    //    }
    //}

    IEnumerator LoadLevelAsync(string levelName, bool autoHideLoadingScreen)
    {
        _isLoading = true;

#if UNITY_WEBPLAYER
        //yield return StartCoroutine(DownloadLevelAsync(levelName));
#endif
        float loadProgress = 0.0f;
        AsyncOperation status = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(levelName);
        status.allowSceneActivation = false;

        // Wait until done and collect progress as we go.
        while (!status.isDone)
        {
            loadProgress = status.progress;

            //if (ProgressBar != null)
            //    ProgressBar.value = loadProgress;

            if (loadProgress >= 0.9f)
            {
                // Almost done.
                break;
            }
            yield return null;
        }

        //if (ProgressBar != null)
        //    ProgressBar.value = 1.0f;

        // Allow new scene to start.
        status.allowSceneActivation = true;

        yield return status;

        //// If the loading screen was shown, hide it.
        //if (LoadingScreenManager && LoadingScreenManager.IsLoadScreenShown() && autoHideLoadingScreen)
        //{
        //    LoadingScreenManager.Hide();

        //    yield return new WaitForEndOfFrame();
        //    yield return new WaitForEndOfFrame();

        //    while (LoadingScreenManager.IsAnimating())
        //        yield return null;
        //}

        _isLoading = false;
    }
}