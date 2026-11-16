using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PanelGroup : MonoBehaviour
{
    public GameObject[] panels;

    public TabGroup tabGroup;

    public int panelIndex;

    private void Awake()
    {
        ShowCurrentPanel();
    }
    //Show the panel that is selected on the tabGroup
    void ShowCurrentPanel()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == panelIndex)
            {
                panels[i].gameObject.SetActive(true);
            }
            else
            {
                panels[i].gameObject.SetActive(false);
            }

        }
    }
    //Change the index of the page
    public void SetPageIndex(int index)
    {
        panelIndex = index;
        ShowCurrentPanel();
    }
}
