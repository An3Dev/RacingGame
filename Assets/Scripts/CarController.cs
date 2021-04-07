using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarInput))]

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] float maxForwardVelocity = 10;
    [SerializeField] float maxReverseVelocity = 10;

    [SerializeField] float accelerationMagnitude = 1;
    [Tooltip("The acceleration amount based on the time the car has been accelerating")]
    [SerializeField] AnimationCurve accelerationOverSpeed;
    [SerializeField] float reverseAccelerationMagnitude = 1;


    [Tooltip("The higher the friction, the faster the car stops when engine is idle.")]
    [SerializeField] float friction = 5;
    [SerializeField] float sideFriction = 20;
    [SerializeField] float brakeForce = 30;
    [SerializeField] float drag = 0;
    [SerializeField] float airDrag = 1;
    [SerializeField] float angularDrag = 1;
    [SerializeField] float postLandingAngularDrag = 1000;



    [SerializeField] float turnAmount = 10;
    [SerializeField] float speedAtWhichTurningSlows = 3;
    [SerializeField] float maxTurnValue = 100;
    [SerializeField] float turnSlowDownAmount = 10;

    [SerializeField] float sideRotationForce;
    [SerializeField] float timeInAirBeforeCanAirRoll = 0.2f;
    [SerializeField] float postLandingRotationLockTime = 0.2f;
    float timeSinceLeftAirTimer = 0;
    float postLandingTimer = -1;

    [SerializeField] Transform centerOfMassTransform;
    [SerializeField] Transform centerOfMassWhileInAirTransform;

    [Range(0, 49.99f)]
    [Tooltip("At 49, it takes the longest time to return to having the steering wheel straight. At 0, the steering wheel is straight instantly.")]
    [SerializeField] float straightenOutSteeringWheelMultiplier = 1;

    Rigidbody rb;

    [SerializeField] Transform[] groundCheck;
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] int numWheelsOnGroundConsideredInAir = 2;

    float currentTurnValue = 0;
    bool wasInAirLastFrame = false;

    Vector3 localVelocity = Vector3.zero;

    public int numTiresOnGround;

    // input
    bool isDriftPressed = false;
    float gasInput, brakeInput;
    float steerInput;
    Vector2 airRollInput;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMassTransform.localPosition;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
    }

    #region Receive Input
    public void ReceiveGasInput(float value)
    {
        gasInput = value;
    }

    public void ReceiveSteeringInput(float value)
    {
        steerInput = value;
    }

    public void ReceiveAirRollInput(Vector2 value)
    {
        airRollInput = value;
    }

    public void ReceiveBrakeInput(float value)
    {
        brakeInput = value;
    }

    public void ReceiveDriftInput(bool value)
    {
        isDriftPressed = value;
    }

    #endregion

    private void Update()
    {
        numTiresOnGround = 0;
        for (int i = 0; i < groundCheck.Length; i++)
        {
            Collider[] colliders = Physics.OverlapSphere(groundCheck[i].position, groundCheckRadius);
            foreach (Collider c in colliders)
            {
                if (!c.transform.root.Equals(transform))
                {
                    numTiresOnGround++;
                    break;
                }
            }
        }

        // if car is in the air
        if (numTiresOnGround <= numWheelsOnGroundConsideredInAir)
        {   
            // if the car just became airborne.
            if (!wasInAirLastFrame)
            {
                rb.centerOfMass = centerOfMassWhileInAirTransform.localPosition;
                rb.drag = airDrag;
                localVelocity = Vector3.zero;
            }

            timeSinceLeftAirTimer += Time.deltaTime;
            wasInAirLastFrame = true;

            return;
        }
        else // if the car is on the ground
        {
            //if the car just landed
            if (wasInAirLastFrame)
            {
                Debug.Log("Set velocity from air");
                localVelocity = transform.InverseTransformVector(rb.velocity);
                localVelocity.y = 0;
                wasInAirLastFrame = false;
                rb.centerOfMass = centerOfMassTransform.localPosition;
                rb.drag = drag;

                timeSinceLeftAirTimer = 0;
                postLandingTimer = 0;
            }
            // if it's time to unlock rotation
            if (postLandingTimer >= postLandingRotationLockTime)
            {
                rb.angularDrag = angularDrag;
                postLandingTimer = -1;
            }
            else if (postLandingTimer != -1)
            {
                rb.angularDrag = angularDrag + (postLandingTimer / postLandingRotationLockTime) * (postLandingAngularDrag - angularDrag);
                postLandingTimer += Time.deltaTime;
            }
        }

        Steering();

        // if there is input
        if (gasInput != 0 || brakeInput != 0)
        {
            // limit the velocity
            //if (localVelocity.z > maxForwardVelocity)
            //{
            //    localVelocity.z = maxForwardVelocity;
            //}
            //else if (localVelocity.z < -maxReverseVelocity)
            //{
            //    localVelocity.z = -maxReverseVelocity;
            //}

            // braking
            if (localVelocity.z > 0 && brakeInput != 0)
            {
                localVelocity.z -= brakeForce * brakeInput * Time.deltaTime;
                Debug.Log("Brake");
            }

            // if user wants to go in reverse
            if (localVelocity.z <= 0 && brakeInput > 0)
            {
                localVelocity -= Vector3.forward * reverseAccelerationMagnitude * brakeInput * accelerationOverSpeed.Evaluate(-localVelocity.z / maxReverseVelocity) * Time.deltaTime;

                Debug.Log("Reverse");

            }
            else if (gasInput > 0)
            {
                //if (localVelocity.z < maxForwardVelocity)
                //{
                localVelocity += Vector3.forward * accelerationMagnitude * gasInput * accelerationOverSpeed.Evaluate(localVelocity.z / maxForwardVelocity) * Time.deltaTime;
                //}
                Debug.Log("Forward");
            }
        }

        // if there's no input, and the car is not moving downhill, slow down the car
        if (gasInput == 0 && localVelocity.z != 0 && rb.velocity.y > -1)
        {
            int slowDownDir = 1;
            if (localVelocity.z < 0)
            {
                slowDownDir = -1;
            }

            localVelocity.z -= slowDownDir * friction * Time.deltaTime;
            if (Mathf.Abs(localVelocity.z) < 0.1f)
            {
                localVelocity.z = 0;
            }


        }
        if (!isDriftPressed && localVelocity.x != 0)
        {
            int slowDownDir = 1;
            if (localVelocity.x < 0)
            {
                slowDownDir = -1;
            }

            localVelocity.x -= slowDownDir * sideFriction * Time.deltaTime;
            if (Mathf.Abs(localVelocity.x) < 0.1f)
            {
                localVelocity.x = 0;
            }
        }
        else if (isDriftPressed)
        {
            Debug.Log("Drift");
        }


        Vector3 worldVelocity = transform.TransformVector(localVelocity);
        worldVelocity.y = rb.velocity.y;
        rb.velocity = worldVelocity;
    }

    private void Steering()
    {
        int turnDir = 1;
        if (localVelocity.z < 0)
            turnDir = -1;

        float magnitude = rb.velocity.magnitude;
        float turningMultiplier = (Mathf.Abs(magnitude) > speedAtWhichTurningSlows) ? 1 : Mathf.Abs(magnitude / speedAtWhichTurningSlows);

        // if player wants to turn left, but car is turning right, and vice versa
        if ((steerInput > 0 && currentTurnValue < 0) || (steerInput < 0 && currentTurnValue > 0))
        {
            //currentTurnValue = 0;
            currentTurnValue += steerInput * Time.deltaTime * 100;
        }
        else
        {
            if (steerInput != 0)
            {
                currentTurnValue += steerInput * Time.deltaTime * 100;
            }
            else
            {
                float val = straightenOutSteeringWheelMultiplier * Time.deltaTime;
                val = Mathf.Clamp(val, 0, 0.99f);
                currentTurnValue *= val;
            }
        }

        // clamp turn value
        if (currentTurnValue > maxTurnValue)
        {
            currentTurnValue = maxTurnValue;
        }
        else if (currentTurnValue < -maxTurnValue)
        {
            currentTurnValue = -maxTurnValue;
        }
        Vector3 turningValuePercent = new Vector3(0, currentTurnValue / maxTurnValue, 0);
        localVelocity.z -= turningMultiplier * Mathf.Abs(turningValuePercent.y) * turnSlowDownAmount * Time.deltaTime;
        transform.Rotate(turningValuePercent * turnDir * turnAmount * turningMultiplier * Time.deltaTime, Space.Self);
    }

    private void FixedUpdate()
    {
        if (numTiresOnGround <= numWheelsOnGroundConsideredInAir)
        {
            if (airRollInput != Vector2.zero && timeSinceLeftAirTimer > timeInAirBeforeCanAirRoll)
            {
                rb.AddRelativeTorque(Vector3.forward * -airRollInput.x * sideRotationForce, ForceMode.Acceleration);
                rb.AddRelativeTorque(Vector3.right * airRollInput.y * sideRotationForce, ForceMode.Acceleration);
            }
        }
    }
}
