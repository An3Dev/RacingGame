using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarCreation", order = 1)]
public class CarSettings : ScriptableObject
{
    public float maxForwardVelocity;
    public float maxReverseVelocity;
    public float accelerationMagnitude;
    public AnimationCurve accelerationOverSpeed;
    public float reverseAccelerationMagnitude;
    public float tireSpinAmount;

    public float driftAcceleration = 15;
    public AnimationCurve driftAccelerationOverSpeed;

}
