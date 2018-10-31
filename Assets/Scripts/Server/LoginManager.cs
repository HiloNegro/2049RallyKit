using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Newtonsoft.Json.Linq;

public class LoginManager : MonoBehaviour
{
    public CanvasGroup LoginPanel;
    public TMP_InputField UsernameInputField;
    public TMP_InputField PasswordInputField;
    public Button LoginButton;
    public LoadingCircle LoadingPanel;
    public TextMeshProUGUI loadingLabel;
    public TextMeshProUGUI errorText;

    public ProjectConstants.SceneNames SceneToLoadOnLogIn = ProjectConstants.SceneNames.Main;

    private void Start()
    {
        string username = UsernameInputField.text = ProgressTracker.Instance.ProgressState.username;
        string password = PasswordInputField.text = ProgressTracker.Instance.ProgressState.password;
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            StartCoroutine(ServerLoginLocal(username, password));
            return;
        }

        RequestAuthentication();
    }

    private void OnEnable()
    {
        LoginButton.onClick.AddListener(ServerLoginForm);
        UsernameInputField.onValueChanged.AddListener(ValidateLoginData);
        PasswordInputField.onValueChanged.AddListener(ValidateLoginData);
    }

    private void OnDisable()
    {
        LoginButton.onClick.RemoveListener(ServerLoginForm);
        UsernameInputField.onValueChanged.RemoveListener(ValidateLoginData);
        PasswordInputField.onValueChanged.RemoveListener(ValidateLoginData);
    }

    private void ValidateLoginData(string value)
    {
        LoginButton.interactable = !string.IsNullOrEmpty(UsernameInputField.text) && !string.IsNullOrEmpty(PasswordInputField.text);
    }

    private void ServerLoginForm()
    {
        errorText.alpha = 0.0f;

        PasswordInputField.interactable = UsernameInputField.interactable = LoginButton.interactable = false;
        string user = UsernameInputField.text;
        string pswd = PasswordInputField.text;

        StartCoroutine(ServerLoginLocal(user, pswd));
    }

    private IEnumerator ServerLoginLocal(string user, string pswd)
    {
        LoadingPanel.Show(true);
        yield return new WaitForSeconds(LoadingPanel.FadeTime);

        if (user.Equals("Centro") && pswd.Equals("centroar"))
        {
            LoadingPanel.Show(false);
            yield return new WaitForSeconds(LoadingPanel.FadeTime);

            ServerAuthenticationResponse(true);

            yield break;
        }


        LoadingPanel.Show(false);
        yield return new WaitForSeconds(LoadingPanel.FadeTime);

        errorText.alpha = 1.0f;
        ServerAuthenticationResponse(false);
    }

    private IEnumerator ServerLogin(string user, string pswd)
    {
        LoadingPanel.Show(true);
        yield return new WaitForSeconds(LoadingPanel.FadeTime);

        using (UnityWebRequest www = UnityWebRequest.Get("http://enjambre.cultura.gob.mx/api/retos/2049-rally-en-tu-ciudad/" + user + "/" + pswd))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                JObject entry = JObject.Parse(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.text);

                if (entry["mensaje"].Value<string>().Equals("ok"))
                {
                    if (entry["entry"]["jsonstatus"] == null)
                    {
                        ProgressTracker.Instance.ResetProgress();
                    }
                    else
                    {
                        Debug.Log(entry["entry"]["jsonstatus"].Value<string>());
                        ProgressTracker.Instance.DeserializeProgress(entry["entry"]["jsonstatus"].Value<string>());
                    }

                    ProgressTracker.Instance.ProgressState.entryId = entry["entry"]["_id"].Value<string>();
                    ProgressTracker.Instance.ProgressState.sessionToken = entry["entry"]["sesiontoken"].Value<string>();

                    LoadingPanel.Show(false);
                    yield return new WaitForSeconds(LoadingPanel.FadeTime);

                    ServerAuthenticationResponse(true);

                    yield break;
                }
            }
        }

        LoadingPanel.Show(false);
        yield return new WaitForSeconds(LoadingPanel.FadeTime);

        errorText.alpha = 1.0f;
        ServerAuthenticationResponse(false);
    }

    private void ServerAuthenticationResponse(bool authenticated)
    {
        if (authenticated)
        {
            ProgressTracker.Instance.ProgressState.username = UsernameInputField.text;
            ProgressTracker.Instance.ProgressState.password = PasswordInputField.text;
            ProgressTracker.Instance.SaveProgress();
            OnServerAuthenticated();
        }
        else
        {
            RequestAuthentication();
        }
    }

    private void OnServerAuthenticated()
    {
        PasswordInputField.interactable = UsernameInputField.interactable = LoginPanel.interactable = LoginPanel.blocksRaycasts = LoginButton.interactable = false;
        LoginPanel.DOFade(0.0f, 1.0f).OnComplete(() =>
        {
            LoadingPanel.Show(true);
            loadingLabel.alpha = 1.0f;
            StreamLoader.Instance.Load(StringEnum.GetStringValue(SceneToLoadOnLogIn));
        });
    }

    private void RequestAuthentication()
    {
        PasswordInputField.interactable = UsernameInputField.interactable = LoginPanel.interactable = LoginPanel.blocksRaycasts = true;
        LoginPanel.DOFade(1.0f, 0.5f).OnComplete(() =>
        {
            ValidateLoginData("");
        });
    }
}