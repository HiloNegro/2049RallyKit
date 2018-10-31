using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Enumeration with the possible Game States available. This states are only valid during gameplay.
/// </summary>
public enum GameState
{
    /// <summary>
    /// The game is Loading
    /// </summary>
    LOADING,
    /// <summary>
    /// The user is currently playing the game.
    /// </summary>
    PLAYING,
    /// <summary>
    /// The user has paused the game.
    /// </summary>
    PAUSED,
    /// <summary>
    /// The user has lost the current game.
    /// </summary>
    GAME_LOST,
    /// <summary>
    /// The user has won the current game
    /// </summary>
    GAME_WON
};

/// <summary>
/// The Game State Manager keeps track of the Current Game State
/// and is responsible transitioning to valid states
/// upon request.
/// This is a manager class and inherits from Singleton
/// </summary>
[AddComponentMenu("Caldera/System/Game State Manager")]
public class GameStateManager : Singleton<GameStateManager>
{
    /// <summary>
    /// The current state in which the game is at
    /// </summary>
    private GameState _currentState;
    public GameState CurrentState
    {
        get
        {
            return Instance._currentState;
        }
    }

    /// <summary>
    /// The previous state in which the game was at
    /// </summary>
    private GameState _previousState;
    public GameState PreviousState
    {
        get
        {
            return _previousState;
        }
    }

    /// <summary>
    /// When the manager is created, set the initial
    /// game state to INITIALIZING.
    /// </summary>
    public override void Init()
    {
        _currentState = GameState.LOADING;
        _previousState = GameState.LOADING;
    }

    public bool RequestGameState(GameState state)
    {
        bool stateChanged = false;
        switch (state)
        {
            case GameState.PLAYING:
                stateChanged = RequestPlayingState();
                break;

            case GameState.GAME_LOST:
            case GameState.GAME_WON:
                stateChanged = RequestGameCompletedState(state);
                break;

            case GameState.PAUSED:
                stateChanged = RequestPauseState();
                break;

            case GameState.LOADING:
                stateChanged = RequestLoadingState();
                break;
            default:
                break;
        }

        return stateChanged;
    }

    private bool RequestPlayingState()
    {
        if (_currentState == GameState.PLAYING) return true;

        if (_currentState == GameState.LOADING ||
            _currentState == GameState.PAUSED)
        {
            _previousState = _currentState;
            _currentState = GameState.PLAYING;
            return true;
        }

        return false;
    }

    private bool RequestGameCompletedState(GameState state)
    {
        if (_currentState == GameState.PLAYING)
        {
            _previousState = _currentState;
            _currentState = state;
            return true;
        }

        return false;
    }

    private bool RequestPauseState()
    {
        if (_currentState == GameState.PAUSED) return true;

        if (_currentState == GameState.PLAYING)
        {
            _previousState = _currentState;
            _currentState = GameState.PAUSED;

            return true;
        }

        return false;
    }

    private bool RequestLoadingState()
    {
        if (_currentState == GameState.LOADING) return true;

        if (_currentState == GameState.GAME_LOST ||
            _currentState == GameState.GAME_WON ||
            _currentState == GameState.PLAYING ||
            _currentState == GameState.PAUSED)
        {
            _previousState = _currentState;
            _currentState = GameState.LOADING;

            return true;
        }

        return false;
    }
}