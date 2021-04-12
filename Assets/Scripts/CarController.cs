using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarInput))]

public class CarController : MonoBehaviour
{
    [Header("Objects to Assign")]
    public CarCamera carCam;
    [SerializeField] Transform frontRightWheel;
    [SerializeField] Transform frontLeftWheel;
    [SerializeField] Transform backLeftWheel;
    [SerializeField] Transform backRightWheel;

    [SerializeField] Transform centerOfMassTransform;
    [SerializeField] Transform centerOfMassWhileInAirTransform;

    [Space]

    [Header("Car Settings")]
    [Space]
    [Header("Speed")]
    [SerializeField] float maxForwardVelocity = 10;
    [SerializeField] float maxReverseVelocity = 10;   
    [SerializeField] float accelerationMagnitude = 1;
    [Tooltip("The acceleration amount based on the time the car has been accelerating")]
    [SerializeField] AnimationCurve accelerationOverSpeed;
    [SerializeField] float reverseAccelerationMagnitude = 1;

    [Header("Stop Forces")]
    [Tooltip("The greater the friction, the faster the car stops when engine is idle.")]
    [SerializeField] float friction = 5;
    [SerializeField] float sideFriction = 20;
    [SerializeField] float driftSideFriction = 0.01f;

    [SerializeField] float brakeForce = 30;
    [SerializeField] float drag = 0;
    [SerializeField] float airDrag = 1;
    [SerializeField] float angularDrag = 1;
    [SerializeField] float postLandingAngularDrag = 1000;

    [Space]
    [Header("Steer")]
    [SerializeField] float turnAmount = 10;
    [SerializeField] float speedAtWhichTurningSlows = 3;
    [SerializeField] float maxTurnValue = 100;
    [SerializeField] float turnSlowDownAmount = 10;
    [Range(0, 500)]
    [Tooltip("The higher the value, the faster the steering wheel goes back to position")]
    [SerializeField] float straightenOutSteeringWheelMultiplier = 1;
    [SerializeField] bool isUsingAnalogSteering = false;


    [Header("Air Roll")]
    [SerializeField] float sideRotationForce;
    [SerializeField] float timeInAirBeforeCanAirRoll = 0.2f;
    [SerializeField] float postLandingRotationLockTime = 0.2f;

    Rigidbody rb;

    [SerializeField] Transform[] groundCheck;
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] int numWheelsOnGroundConsideredInAir = 2;

    float currentTurnValue = 0;
    bool wasInAirLastFrame = false;
    bool isInAir;
    Vector3 localVelocity = Vector3.zero;

    public int numTiresOnGround;

    float timeSinceLeftAirTimer = 0;
    float postLandingTimer = -1;

    // input
    bool isDriftPressed = false;
    float gasInput, brakeInput;
    float steerInput;
    Vector2 airRollInput;
    float driftDir = 0;

    float lastSteerValue = 0;

    Vector3 worldDriftVelocity;
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
        // detect if controller is using analog or mouse and keyboard.
        //if (lastSteerValue != 0)
        //{
        //    if (!isUsingAnalogSteering)
        //    {
        //        if (lastSteerValue != 1 || lastSteerValue != -1)
        //        {
        //            isUsingAnalogSteering = true;
        //        }
        //    } else
        //    {
        //        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S))
        //        {
        //            isUsingAnalogSteering = false;
        //        }
        //    }
        //}
        

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
            isInAir = true;
        }
        else // if the car is on the ground
        {
            isInAir = false;
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

        if (gasInput != 0 || !isInAir)
        {
            // spin back wheels

            backLeftWheel.Rotate(Vector3.up * (localVelocity.z / maxForwardVelocity) * 360, Space.Self);
            backRightWheel.Rotate(Vector3.up * (localVelocity.z / maxForwardVelocity) * 360, Space.Self);

        }

        if (isInAir)
        {
            return;
        }
        //--------- This Code Does Not Execute While in the Air ------- //

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

        if (isDriftPressed)
        {
            // set the drift direction
            if (driftDir == 0 && steerInput != 0)
            {
                driftDir = (steerInput > 0) ? 1 : -1;
            }

            if (driftDir != 0)
            {
                 // turn car toward drift direction
            }

        } else
        {
            driftDir = 0;
        }

        if (localVelocity.x != 0)
        {
            int slowDownDir = 1;
            if (localVelocity.x < 0)
            {
                slowDownDir = -1;
            }
            float f = isDriftPressed ? driftSideFriction : sideFriction;
            localVelocity.x -= slowDownDir * f * Time.deltaTime;
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
        float turningMultiplier = (magnitude > speedAtWhichTurningSlows) ? 1 : Mathf.Abs(magnitude / speedAtWhichTurningSlows);

        float turningValuePercent;

        if (!isUsingAnalogSteering)
        {
            // if player wants to turn left, but car is turning right, and vice versa
            //if ((steerInput > 0 && currentTurnValue < 0) || (steerInput < 0 && currentTurnValue > 0))
            //{
            //    //currentTurnValue = 0;
            //    currentTurnValue += steerInput * Time.deltaTime * 100;
            //}
            //else
            //{
                
            //}

            if (steerInput != 0)
            {
                currentTurnValue += steerInput * Time.deltaTime * 100;
            }
            else
            {
                if (Mathf.Abs(currentTurnValue) > straightenOutSteeringWheelMultiplier * Time.deltaTime)
                {
                    float val = straightenOutSteeringWheelMultiplier * Time.deltaTime;
                    //val = Mathf.Clamp(val, 0, 0.999f);
                    currentTurnValue -= val * Mathf.Sign(currentTurnValue);
                } else
                {
                    currentTurnValue = 0;
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

            turningValuePercent = currentTurnValue / maxTurnValue;
        } 
        // else if user is using analog steering
        else
        {
            turningValuePercent = steerInput;
        }  

        float zRot = 90 + 30 * turningValuePercent;
        frontRightWheel.localRotation = Quaternion.Euler(new Vector3(frontRightWheel.localRotation.eulerAngles.x, 0, zRot));
        frontLeftWheel.localRotation = Quaternion.Euler(new Vector3(frontLeftWheel.localRotation.eulerAngles.x, 0, zRot));

        if (!isInAir)
        {
            localVelocity.z -= turningMultiplier * Mathf.Abs(turningValuePercent) * turnSlowDownAmount * Time.deltaTime;
            transform.Rotate(Vector3.up * turningValuePercent * turnDir * turnAmount * turningMultiplier * Time.deltaTime, Space.Self);
        }
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
