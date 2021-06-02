using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class TestingManager : MonoBehaviour
{
    public static int frameRate = 60;

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            frameRate -= 10;
        } else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            frameRate += 10;
        }
        else if(Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            frameRate = 60;
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            frameRate = 144;
        }
        Application.targetFrameRate = frameRate;
    }
}
