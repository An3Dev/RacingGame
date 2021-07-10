using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelContainer : MonoBehaviour
{
    GameObject currentlyVisiblePanel;
    GameObject[] panels;

    private void Awake()
    {
        panels = new GameObject[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            panels[i] = transform.GetChild(i).gameObject;
        }
    }
    public void ShowPanel(GameObject panel)
    {
        if (currentlyVisiblePanel != null || currentlyVisiblePanel != panel)
        {
            for(int i = 0; i < panels.Length; i++)
            {
                if (!panels[i].Equals(panel))
                {
                    panels[i].SetActive(false);
                }
            }
            panel.SetActive(true);
            currentlyVisiblePanel = panel;
        }
    }
}
