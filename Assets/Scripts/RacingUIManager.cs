using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RacingUIManager : MonoBehaviour
{
    public static RacingUIManager Instance;
    public TextMeshProUGUI timer;
    public TextMeshProUGUI speed;
    public CarController carController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(this);
        }
        //SetTimerText(61.5f);

        InvokeRepeating(nameof(UpdateSpeedText), 0.001f, 0.05f);
    }

    void UpdateSpeedText()
    {
        speed.text = carController.GetSpeed().ToString("00");
    }

    public void SetTimerText(string formattedText)
    {     
        timer.text = formattedText;
    }
}
