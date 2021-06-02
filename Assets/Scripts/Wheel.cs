using UnityEngine;

//Attach this script to a wheel bone. It will move the visual wheel to the position of the wheel collider
public class Wheel : MonoBehaviour
{
    public Transform correspondingWheel;

    private void Update()
    {
        transform.position = correspondingWheel.position;
    }
}
