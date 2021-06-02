using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    public void SetCountdownText(string text)
    {
        RacingUIManager.Instance.SetCountdownText(text);
    }
}
