using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ObjectSelectButton : MonoBehaviour
{

    public GameObject bg;
    public GameObject selectedBg;
    public TextMeshProUGUI objectNameText;
    public Image objectImage;
    public string objectName;

    public void OnClick()
    {
        LeaderboardUIManager.Instance.OnCarButtonClicked(transform.GetSiblingIndex());
        selectedBg.SetActive(true);
    }

    public void SetSelected(bool selected)
    {
        bg.SetActive(!selected);
        selectedBg.SetActive(selected);
    }
}
