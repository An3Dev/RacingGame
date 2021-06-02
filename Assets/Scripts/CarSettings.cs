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
    [Tooltip("The acceleration amount based on the time the car has been accelerating")]
    public AnimationCurve accelerationOverSpeed;
    public float reverseAccelerationMagnitude = 30;
    [Tooltip("Degrees the tires spin per second at 1 meter per second. At 2 meters per second, the tires will spin twice as fast.")]
    public float tireSpinAmount = 360;

    [Header("Drifting")]
    [Tooltip("This only applies when the user is not pressing the gas or the brake/reverse. " +
        "If the forward speed of the car is bigger than this value, then the wheels will start spinning. " +
        "This is so that the wheels don't spin when the car is perpendicular to its velocity vector.")]
    public float driftAcceleration = 15;
    public AnimationCurve driftAccelerationOverSpeed;
    public float driftFriction = 3;
    public float lateralDriftFriction = 1;
    public float driftingTurnAmount = 115;
    //public float speedWhereWheelsStartSpinningWhenDrifting = 5;

    //public float angleBelowNoDriftForceIsAdded = 10;
    
    //public float angleWhereFullDriftForceIsAdded = 20;
    //public float speedAtWhichDriftAccelerationStops = 20;


    public float driftTireSpinAmount = 1440;

    [Header("Stop Forces")]
    [Tooltip("The greater the friction, the faster the car stops when engine is idle.")]
    public float friction = 5;
    public float lateralFriction = 20;
    public float brakeForce = 20;
    public float driftBrakeForce = 10;
    public float drag = 0;
    public float airDrag = 0.05f;
    public float angularDrag = 1;
    public float postLandingAngularDrag = 1;
    public float downForce = 1000;

    [Header("Steer")]
    public float turnAmount = 45;
    public float speedAtWhichTurningSlows = 1;
    //public float maxTurnValue = 10;
    [Tooltip("The time it takes to fully turn to turnAmount. This only applies to PC because it is digital input. Controller players can control how much the car turns by the amount that they move their joystick.")]
    public float fullSteerTime = 0.5f;
    [Tooltip("The time it takes for the wheels to go to normal rotation after player releases steering wheel. This is the time it takes from full turn to 0 turn. Only for Keyboard and Mouse.")]
    public float straightenOutWheelTime = 0.1f;
    [Tooltip("How much the car slows down every second that full steering is applied. If steering is not at max, a fraction of this value will be used to slow down the car.")]
    public float turnSlowDownAmount = 3;
    //[Tooltip("The higher the value, the faster the steering wheel goes back to position")]
    //public float straightenOutSteeringWheelMultiplier = 100;



    [Header("Air Roll")]
    public float sideRotationForce = 7;
}
