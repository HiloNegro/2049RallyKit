using UnityEngine;
using System.Collections;

[AddComponentMenu("Caldera/Game Managers/Game Initialization")]
public class GameInitialization : ActionEvent
{
    /// <summary>
    /// First scene in the game
    /// </summary>
    public ProjectConstants.SceneNames SceneToLoad = ProjectConstants.SceneNames.Login;

    public override void InstantTriggerAction()
    {
        base.InstantTriggerAction();

        Application.targetFrameRate = 20;

        if (Application.isMobilePlatform)
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public override void DelayedTriggerAction()
    {
        base.DelayedTriggerAction();

        StreamLoader.Instance.Load(StringEnum.GetStringValue(SceneToLoad));
    }
}