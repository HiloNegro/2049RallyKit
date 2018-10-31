using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameTimeManager : Singleton<GameTimeManager>
{
    private float _timeSceneWasLoaded = 0.0f;

    /// <summary>
    /// The time required for the game to enter an idle state
    /// </summary>
    [SerializeField]
    protected float _timeToIdle = 120.0f;
    protected float _idleCounter = 0.0f;
    protected float _totalIdleTimeInScene = 0.0f;

    private bool _paused = false;

    public float PlayTime
    {
        get { return (Time.time - _timeSceneWasLoaded) - _totalIdleTimeInScene; }
    }

    public float IdleTime
    {
        get { return _totalIdleTimeInScene; }
    }

    public float IdleTimer
    {
        get { return _idleCounter; }
    }

    public override void Init()
    {
        base.Init();

        _totalIdleTimeInScene = 0.0f;
        _timeSceneWasLoaded = 0.0f;
        _idleCounter = 0.0f;
    }

    void OnEnable()
    {
        SceneManager.activeSceneChanged += SceneChanged;
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= SceneChanged;
    }

    private void SceneChanged(Scene previousScene, Scene currentScene)
    {
        _timeSceneWasLoaded = Time.time;
        _totalIdleTimeInScene = 0.0f;
        _idleCounter = 0.0f;
    }

    void Update()
    {
        if(Input.anyKey)
        {
            _paused = false;

            if (_idleCounter > _timeToIdle)
                _totalIdleTimeInScene += _idleCounter;

            _idleCounter = 0.0f;
            
            return;
        }

        if (!_paused)
            _idleCounter += Time.deltaTime;
    }

    public void PauseIdleCounter()
    {
        _paused = true;
    }

    public void ResetIdleCounter()
    {
        _idleCounter = 0.0f;
        _totalIdleTimeInScene = 0.0f;
    }
}