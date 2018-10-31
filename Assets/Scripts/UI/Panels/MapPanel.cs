using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapPanel : AppPanel
{
    public Map map;
    public TextMeshProUGUI locationText;
    public Button retryButton;
    public LoadingCircle loadingIndicator;

    [TextArea]
    public string activateGPSMessage;
    [TextArea]
    public string timeOutGPSMessage;
    [TextArea]
    public string failedGPSMessage;

    private bool usingLocationService = false;


    private float debugTouchTime = 0.0f;
    private float debugTouchCount = 0;

    protected override void Start()
    {
        base.Start();

        loadingIndicator.Show(false);
        retryButton.gameObject.SetActive(false);
        locationText.gameObject.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        retryButton.onClick.AddListener(RefreshLocationService);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        retryButton.onClick.RemoveListener(RefreshLocationService);
    }

    private void OnDestroy()
    {
        usingLocationService = false;
        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }

    protected override void TweenSetup()
    {
        //float _panelHeight = _transform.sizeDelta.y;
        //_tweenPosition.initialValue = new Vector2(_transform.anchoredPosition.x, -_panelHeight);
        //_tweenPosition.finalValue = _transform.anchoredPosition;
        //_tweenPosition.ResetTween();

        //_tweenAlpha.initialValue = 0.0f;
        //_tweenAlpha.finalValue = 1.0f;
        //_tweenAlpha.ResetTween();
        _tweenPosition.initialValue = new Vector2(_transform.anchoredPosition.x, _transform.anchoredPosition.y - 50.0f);
        _tweenPosition.finalValue = _transform.anchoredPosition;
        _tweenPosition.ResetTween();

        _tweenAlpha.initialValue = 0.0f;
        _tweenAlpha.finalValue = 1.0f;
        _tweenAlpha.ResetTween();
    }

    public override void ShowPanel(bool show)
    {
        RefreshLocationService();

        base.ShowPanel(show);

        map.ShowMap(show && usingLocationService);
    }

    public void RefreshLocationService()
    {
        loadingIndicator.Show(false);
        retryButton.gameObject.SetActive(false);
        locationText.gameObject.SetActive(false);

        if (!usingLocationService)
            StartCoroutine(RunLocationServices());
    }

    public void StartMainActivity(int index, bool activate)
    {
        map.ActivateLocation(index, activate);
    }

    public void StartSecondaryActivities(int index, bool activate)
    {
        map.ActivateLocationEvents(index, activate);
    }

    public bool HasReachedTarget(double latitude, double longitude, float radius)
    {
        // TODO: Remove this code. Debug only.
        if (Input.GetKey(KeyCode.Space) || Input.touchCount == 3)
        {
            debugTouchTime += 1.0f;

            if (debugTouchTime >= 5.0f)
            {
                debugTouchCount++;
                debugTouchTime = 0.0f;
                Handheld.Vibrate();
            }

            if (debugTouchCount >= 2)
            {
                debugTouchCount = 0;
                debugTouchTime = 0.0f;
                return true;
            }

            return false;
        }

        debugTouchCount = 0.0f;
        debugTouchCount = 0;

        if (!usingLocationService)
            return false;

        return DistanceTo(latitude, longitude, Input.location.lastData.latitude, Input.location.lastData.longitude) * 1000.0 < radius;

    }

    private IEnumerator RunLocationServices()
    {
        // Show loading animation
        loadingIndicator.Show(true);

        // TODO: remove these lines. Only for testing purposes.
        if (Application.isEditor)
            yield return new WaitForSeconds(1.0f);

#if !UNITY_EDITOR
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            locationText.text = activateGPSMessage;
            locationText.gameObject.SetActive(true);
            retryButton.gameObject.SetActive(true);
            loadingIndicator.Show(false);
            yield break;
        }

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            locationText.text = timeOutGPSMessage;
            locationText.gameObject.SetActive(true);
            retryButton.gameObject.SetActive(true);
            loadingIndicator.Show(false);
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            locationText.text = failedGPSMessage;
            locationText.gameObject.SetActive(true);
            retryButton.gameObject.SetActive(true);
            loadingIndicator.Show(false);
            yield break;
        }
        else
        {
#endif
            loadingIndicator.Show(false);

            usingLocationService = true;

            map.ShowMap(usingLocationService && IsPanelActive());
#if !UNITY_EDITOR
        }
#endif
    }

    private double DistanceTo(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        int R = 6371;

        double f1 = Mathf.Deg2Rad * latitude1;
        double f2 = Mathf.Deg2Rad * latitude2;

        double df = Mathf.Deg2Rad * (latitude1 - latitude2);
        double dl = Mathf.Deg2Rad * (longitude1 - longitude2);

        double a = Mathf.Sin((float)(df / 2)) * Mathf.Sin((float)(df / 2)) +
        Mathf.Cos((float)f1) * Mathf.Cos((float)f2) *
        Mathf.Sin((float)(dl / 2)) * Mathf.Sin((float)(dl / 2));

        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));

        double d = R * c;

        return d;
    }
}