using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayerSlot : MonoBehaviour
{ 
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] GameObject readySymbol;

    string playerName;
    bool isReady = false;

    public void SetIsReady(bool isReady)
    {
        this.isReady = isReady;
        readySymbol.SetActive(this.isReady);
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
        nameText.text = playerName;
    }

    public bool GetIsReady()
    {
        return isReady;
    }
}
