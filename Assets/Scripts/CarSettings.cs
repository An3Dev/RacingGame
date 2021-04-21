using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarCreation", order = 1)]
public class CarSettings : ScriptableObject
{

    [Header("Velocity")]

    public float maxForwardVelocity = 70;
    public float maxReverseVelocity = 30;
    public float accelerationMagnitude = 30;
    public AnimationCurve accelerationOverSpeed;
    public float reverseAccelerationMagnitude = 30;
    public float tireSpinAmount = 360;

    [Header("Drifting")]
    public float driftAcceleration = 15;
    public AnimationCurve driftAccelerationOverSpeed;
    public float driftFriction = 3;
    public float lateralDriftFriction = 1;
    public float driftingTurnAmount = 115;
    public float speedWhereWheelsStartSpinningWhenDrifting = 5;
    public float angleBelowNoDriftForceIsAdded = 10;
    public float angleWhereFullDriftForceIsAdded = 20;
    public float speedAtWhichDriftAccelerationStops = 20;

    public float driftTireSpinAmount = 1440;

    [Header("Stop Forces")]
    public float friction = 5;
    public float lateralFriction = 20;
    public float brakeForce = 20;
    public float drag = 0;
    public float airDrag = 0.05f;
    public float angularDrag = 1;
    public float postLandingAngularDrag = 1;

    [Header("Steer")]
    public float turnAmount = 45;
    public float speedAtWhichTurningSlows = 1;
    public float maxTurnValue = 10;
    public float turnSlowDownAmount = 3;
    public float straightenOutSteeringWheelMultiplier = 100;

    [Header("Air Roll")]
    public float sideRotationForce = 7;
}
