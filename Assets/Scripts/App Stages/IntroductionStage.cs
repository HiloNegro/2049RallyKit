using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class IntroductionStage : AppStage
{
    [Header("General Settings")]
    public CanvasGroup tutorialTextGroup;
    private TextMeshProUGUI tutorialTextMesh;
    public float tutorialTextFadeTime = 0.3f;
    public Button tutorialButton;
    private UITweenManager tutorialButtonTweenManager;

    [Header("Welcome section")]
    public string welcomeAudio;
    [TextArea(6, 20)]
    public string welcomeText;

    [Header("Instructions Settings")]
    public AppPanel instructionsPanel;
    public Button instructionsButton;
    public string instructionsAudio;

    [Header("Map Settings")]
    public AppPanel mapPanel;
    public Button mapButton;
    public string mapAudio;

    [Header("AR Settings")]
    public AppPanel arPanel;
    public Button arButton;
    public string arAudio;

    [Header("Future Settings")]
    //public FuturePanel futurePanel;
    public Button futureButton;
    //public string futureAudio;

    [Header("Start Rally Settings")]
    public string startRallyAudio;

    private int currentTutorialSection = -1;
    private bool panelButtonClicked = false;
    private float panelButtonClickTime = 0.0f;

    public void Awake()
    {
        tutorialTextMesh = tutorialTextGroup.GetComponent<TextMeshProUGUI>();
        tutorialButtonTweenManager = tutorialButton.GetComponent<UITweenManager>();
    }

    private void OnEnable()
    {
        tutorialButton.onClick.AddListener(PlayNextSection);
    }

    private void OnDisable()
    {
        tutorialButton.onClick.RemoveListener(PlayNextSection);
    }

    public override void Play(ActivityProgress progress, Action callback)
    {
        onCompletedCallback = callback;

        PlayNextSection();
    }

    private void PlayNextSection()
    {
        currentTutorialSection++;

        switch(currentTutorialSection)
        {
            case 0:
                instructionsButton.interactable = mapButton.interactable = arButton.interactable = futureButton.interactable = false;

                StartCoroutine(WelcomeSection(welcomeAudio, welcomeText));
                break;
            case 1:
                tutorialTextGroup.DOFade(0.0f, tutorialTextFadeTime);
                StartCoroutine(OpenPanelSection(instructionsAudio, instructionsButton));
                break;
            case 2:
                if (instructionsPanel.IsPanelActive())
                    instructionsPanel.ActivatePanel();
                StartCoroutine(OpenPanelSection(mapAudio, mapButton));
                break;
            case 3:
                if (mapPanel.IsPanelActive())
                    mapPanel.ActivatePanel();
                StartCoroutine(OpenPanelSection(arAudio, arButton));
                break;
            case 4:
                if (arPanel.IsPanelActive())
                    arPanel.ActivatePanel();
                StartCoroutine(StartRallySection(startRallyAudio));
                break;
            default:
                tutorialButton.onClick.RemoveListener(PlayNextSection);
                tutorialButtonTweenManager.StartTweens(false);
                instructionsButton.interactable = mapButton.interactable = arButton.interactable = futureButton.interactable = true;

                if (onCompletedCallback != null)
                    onCompletedCallback();

                onCompletedCallback = null;
                break;
        }
    }

    private void AnimatePanelButton(Button panelButton, bool animate)
    {
        Animator buttonAnimator = panelButton.GetComponent<Animator>();
        buttonAnimator.SetBool("Animated", animate);
    }

    private void PanelButtonClicked()
    {
        panelButtonClicked = true;
        panelButtonClickTime = Time.time;
    }

    private IEnumerator WelcomeSection(string audio, string text)
    {
        AudioObject audioObject = AudioManager.Instance.PlayMusicAndSetPlayback(audio);
        tutorialTextMesh.text = text;
        tutorialTextGroup.DOFade(1.0f, tutorialTextFadeTime);

        yield return new WaitForSeconds(audioObject.clipLength);

        tutorialButtonTweenManager.StartTweens(true);
    }

    private IEnumerator OpenPanelSection(string audio, Button panelButton)
    {
        tutorialButtonTweenManager.StartTweens(false);

        AudioObject audioObject = AudioManager.Instance.PlayMusicAndSetPlayback(audio);
        AnimatePanelButton(panelButton, true);
        panelButton.interactable = true;
        panelButton.onClick.AddListener(PanelButtonClicked);
        panelButtonClicked = false;

        while (!panelButtonClicked) yield return null;

        panelButton.interactable = false;

        AnimatePanelButton(panelButton, false);
        panelButton.onClick.RemoveListener(PanelButtonClicked);

        while (audioObject.IsPlaying() || (Time.time - panelButtonClickTime) < 3.0f) yield return null;

        tutorialButtonTweenManager.StartTweens(true);
    }

    private IEnumerator StartRallySection(string audio)
    {
        tutorialButtonTweenManager.StartTweens(false);

        AudioObject audioObject = AudioManager.Instance.PlayMusicAndSetPlayback(audio);

        yield return new WaitForSeconds(audioObject.clipLength);

        tutorialButtonTweenManager.StartTweens(true);
    }
}