using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppPanelManager : MonoBehaviour
{
    private static List<AppPanel> _panelList = new List<AppPanel>();
    private AppPanel[] childPanels;

    private void Awake()
    {
        childPanels = gameObject.GetComponentsInChildren<AppPanel>();
        _panelList.AddRange(childPanels);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < childPanels.Length; i++)
            _panelList.Remove(childPanels[i]);
    }

    private void Start()
    {
        if (childPanels == null) return;

        for (int i = 0; i < childPanels.Length; i++)
        {
            childPanels[i].SetPanelManager(this);
        }
    }

    public void ActivatePanel(AppPanel panel)
    {
        for (int i = 0; i < _panelList.Count; i++)
        {
            _panelList[i].ShowPanel(_panelList[i] == panel);
        }
    }

    public void DeactivateAll()
    {
        for (int i = 0; i < _panelList.Count; i++)
        {
            _panelList[i].ShowPanel(false);
        }
    }
}