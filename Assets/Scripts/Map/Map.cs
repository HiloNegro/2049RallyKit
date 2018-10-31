using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Map : MonoBehaviour
{
    public MapLocation[] locations;

    public CanvasGroup mapGroup;

    public void ShowMap(bool show)
    {
        float targetAlpha = show ? 1.0f : 0.0f;
        mapGroup.DOFade(targetAlpha, 0.2f);

        //for (int i = 0; i < locations.Length; i++)
        //{
        //    locations[i].gameObject.SetActive(show);
        //}
    }

    public void ActivateLocation(int index, bool activate)
    {
        locations[index].ActivateLocation(activate);
    }

    public void ActivateLocationEvents(int index, bool activate)
    {
        locations[index].ActivateLocationEvents(activate);
    }
}