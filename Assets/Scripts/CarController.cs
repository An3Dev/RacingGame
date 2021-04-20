using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarInput))]

public class CarController : MonoBehaviour
{
    [Header("Objects to Assign")]
    public CarCamera carCam;
    [SerializeField] SkinnedMeshRenderer carMesh;
    [SerializeField] Transform frontRightWheel;
    [SerializeField] Transform frontLeftWheel;
    [SerializeField] Transform backLeftWheel;
    [SerializeField] Transform backRightWheel;
    [SerializeField] Transform wheelColliders;


    [SerializeField] Transform frontRightWheelTurnParent;
    [SerializeField] Transform frontLeftWheelTurnParent;

    [SerializeField] Transform centerOfMassTransform;
    [SerializeField] Transform centerOfMassWhileInAirTransform;
    public Checkpoints checkpoints;

    [Space]

    [Header("Car Settings")]

    [SerializeField] CarSettings carSettings;

    [SerializeField] float maxForwardVelocity = 10;
    [SerializeField] float maxReverseVelocity = 10;   
    [SerializeField] float accelerationMagnitude = 1;
    [Tooltip("The acceleration amount based on the time the car has been accelerating")]
    [SerializeField] AnimationCurve accelerationOverSpeed;
    [SerializeField] float reverseAccelerationMagnitude = 1;
    [Tooltip("Degrees the tires spin per second at 1 meter per second. At 2 meters per second, the tires will spin twice as fast.")]
    [SerializeField] float tireSpinAmount = 360;

    [Space]
    [Header("Drifting")]
    [Tooltip("This only applies when the user is not pressing the gas or the brake/reverse. " +
        "If the forward speed of the car is bigger than this value, then the wheels will start spinning. " +
        "This is so that the wheels don't spin when the car is perpendicular to its velocity vector.")]

    [SerializeField] float driftAcceleration = 10;
    [SerializeField] AnimationCurve driftAccelerationOverSpeed;
    [SerializeField] float driftFriction = 0.01f;
    [SerializeField] float lateralDriftFriction = 0.01f;
    [SerializeField] float driftingTurnAmount = 70;
    [SerializeField] float speedWhereWheelsStartSpinningWhenDrifting = 5;

    [Tooltip("If the angle between the car forward direction and the car velocity direction is less than this value, then no force will be added at all.")]
    [SerializeField] float angleBelowNoDriftForceIsAdded = 10;

    [Tooltip("If the angle between the car forward direction and the car velocity direction is greater than or equal to this value, then full drift force will be added." +
        " If the angle is less than this value, then a fraction of the full drift force will be added. If the angle is equal to the angleBelowNoDriftForceIsAdded value, then the smallest amount of drift force will be added. " +
        " If the angle is halfway between the angleBelowNoDriftForceIsAdded value and between this value, the half of the full drift force is added. ")]
    [SerializeField] float angleWhereFullDriftForceIsAdded = 20;
    [SerializeField] float speedAtWhichDriftAccelerationStops = 20;


    [Tooltip("How fast the tires spin when drifting in degrees per second.")]
    [SerializeField] float driftTireSpinAmount = 1440; // degrees per second

    [Header("Stop Forces")]
    [Tooltip("The greater the friction, the faster the car stops when engine is idle.")]
    [SerializeField] float friction = 5;
    [SerializeField] float lateralFriction = 20;

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
    [SerializeField] bool isUsingController = false;


    [Header("Air Roll")]
    [SerializeField] float sideRotationForce = 70;
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

    int numTiresOnGround;

    float timeSinceLeftAirTimer = 0;
    float postLandingTimer = -1;
    bool isDrifting = false;

    // input
    bool isDriftPressed = false;
    float gasInput, brakeInput;
    float steerInput;
    float airRollInput;
    Vector2 airRotInput;

    Vector3 worldDriftVelocity;

    bool airRollIsPressed = false;

    float startTime, endTime;

    float velocitySquareMagnitude;
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

    public void SetIsControllerInput(bool isController)
    {
        isUsingController = isController;
    }

    public void ReceiveAirRollInput(float value)
    {
        airRollInput = value;
    }

    public void ReceiveAirRotInput(Vector2 value)
    {
        airRotInput = value;
    }

    public void ReceiveDoAirRollInput(bool doAirRoll)
    {
        this.airRollIsPressed = doAirRoll;
    }


    public void ReceiveBrakeInput(float value)
    {
        brakeInput = value;
    }

    public void ReceiveDriftInput(bool value)
    {
        isDriftPressed = value;
    }

    public void ResetCarPosition()
    {
        Transform t = checkpoints.GetDefaultCheckpoint();
        transform.position = t.position;
        transform.rotation = t.rotation;
        rb.Sleep();
        rb.WakeUp();
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
        //if (numTiresOnGround >= 1)
        //{
        //    wheelColliders.gameObject.SetActive(true);
        //}
        //else
        //{
        //    wheelColliders.gameObject.SetActive(false);
        //}

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
                //Debug.Log("Set velocity from air");
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


        if (!isInAir && isDriftPressed)
        {
            isDrifting = true;
            worldDriftVelocity = rb.velocity;
        }
        else
        {
            isDrifting = false;
        }

        Steering();
        SpinWheels();

        if (isInAir)
        {
            return;
        }

        //--------- This Code Does Not Execute While in the Air ------- //

        if (!isDrifting)
        {
            ApplyFriction();
        }
        else // is drifting
        {
            ApplyDriftFriction();
        }

        MoveCar();
        
        ApplyVelocityToRigidbody();
    }

    void MoveCar()
    {
        // if there is input
        if (gasInput != 0 || brakeInput != 0)
        {
            // if user wants to brake and we're not drifting.
            if (brakeInput != 0 && !isDrifting)
            {
                // braking
                if (localVelocity.z > 0 && brakeInput != 0)
                {
                    localVelocity.z -= brakeForce * brakeInput * Time.deltaTime;
                }
            }

            // if user wants to move forward
            if (gasInput > 0)
            {
                if (startTime == 0)
                {
                    startTime = Time.time;
                }

                if (!isDrifting)
                {
                    localVelocity += Vector3.forward * accelerationMagnitude * gasInput * accelerationOverSpeed.Evaluate(localVelocity.z / maxForwardVelocity) * Time.deltaTime;
                }
                else
                {
                    AddWorldDriftVelocity(transform.forward, gasInput);
                }
            }
            // if user wants to go in reverse
            else if ((localVelocity.z <= 0.1f || isDrifting) && brakeInput > 0)
            {
                if (!isDrifting)
                {
                    localVelocity -= Vector3.forward * reverseAccelerationMagnitude * brakeInput * accelerationOverSpeed.Evaluate(-localVelocity.z / maxReverseVelocity) * Time.deltaTime;
                }
                else
                {
                    AddWorldDriftVelocity(-transform.forward, brakeInput);                    
                }
            }
        }

        //Debug.DrawRay(transform.position + Vector3.up, rb.velocity.normalized * 5, Color.red, Time.deltaTime);
    }

    void AddWorldDriftVelocity(Vector3 forward, float input)
    {
        float angle = Vector3.Angle(forward, rb.velocity.normalized);

        velocitySquareMagnitude = rb.velocity.sqrMagnitude;
        //Debug.DrawRay(transform.position + Vector3.up, transform.forward * 5, Color.green, Time.deltaTime);

        // if car is moving slowly
        if (velocitySquareMagnitude < speedAtWhichDriftAccelerationStops * speedAtWhichDriftAccelerationStops)
        {
            //Debug.Log("Drift acceleration");
            Vector3 addition = forward * driftAcceleration * driftAccelerationOverSpeed.Evaluate(Mathf.Sqrt(velocitySquareMagnitude) / maxForwardVelocity) * input * Time.deltaTime;

            // if we apply the new velocity and the magnitude is not greater than the max forward velocity, the we actually apply the velocity.
            //if ((worldDriftVelocity + addition).sqrMagnitude < maxForwardVelocity * maxForwardVelocity)
            //{
                worldDriftVelocity += addition;
            //}
        }
        // if car is at an angle to add force against its velocity
        else if (angle > angleBelowNoDriftForceIsAdded)
        {

            float forceMultiplierBasedOnAngle = angle > angleWhereFullDriftForceIsAdded ? 1 : Mathf.Clamp01((angle - angleBelowNoDriftForceIsAdded) / (angleWhereFullDriftForceIsAdded - angleBelowNoDriftForceIsAdded));

            Vector3 addition = forward * forceMultiplierBasedOnAngle * driftAcceleration * driftAccelerationOverSpeed.Evaluate(Mathf.Sqrt(velocitySquareMagnitude) / maxForwardVelocity) * input * Time.deltaTime;
           
            // if we apply the new velocity and the magnitude is not greater than the max forward velocity, the we actually apply the velocity.
            if ((worldDriftVelocity + addition).sqrMagnitude < maxForwardVelocity * maxForwardVelocity)
            {
                //Debug.Log("Add drift force");
                worldDriftVelocity += addition;
            } else
            {
                //Debug.Log("Going too fast. Vector: " + worldDriftVelocity + " + " + addition + " = " + (worldDriftVelocity + addition).ToString("F3") +  ", Mag: " + (worldDriftVelocity + addition).magnitude);
            }
        } else
        {
            //Debug.Log("Angle: " + angle);
        }
    }

    void ApplyVelocityToRigidbody()
    {
        if (!isDrifting)
        {
            Vector3 worldVelocity = transform.TransformVector(localVelocity);
            worldVelocity.y = rb.velocity.y;
            rb.velocity = worldVelocity;
        } else
        {
            // use the world drift velocity that is set in the Drift method
            rb.velocity = worldDriftVelocity;
        }
    }

    void ApplyFriction()
    {
        worldDriftVelocity = Vector3.zero;
        // if there's no input, and the car is not moving downhill, slow down the car
        if (gasInput == 0 && localVelocity.z != 0 && rb.velocity.y > -1)
        {
            int slowDownDir = 1;
            if (localVelocity.z < 0)
            {
                slowDownDir = -1;
            }

            localVelocity.z -= slowDownDir * friction * Time.deltaTime;
            if (Mathf.Abs(localVelocity.z) < 0.01f)
            {
                localVelocity.z = 0;
            }
        }

        if (localVelocity.x != 0)
        {
            int slowDownDir = 1;
            if (localVelocity.x < 0)
            {
                slowDownDir = -1;
            }
            //float f = isDriftPressed ? driftFriction : sideFriction;
            localVelocity.x -= slowDownDir * lateralFriction * Time.deltaTime;
            if (Mathf.Abs(localVelocity.x) < 0.1f)
            {
                localVelocity.x = 0;
            }
        }
    }

    void ApplyDriftFriction()
    {
        Vector3 localDriftVelocity = transform.InverseTransformVector(worldDriftVelocity);
        Vector2 slowDownDir = Vector2.one;

        if (localDriftVelocity.x > 0)
        {
            slowDownDir.x = -1;
        }
        if (localDriftVelocity.z > 0)
        {
            slowDownDir.y = -1;
        }

        localDriftVelocity.x += slowDownDir.x * lateralDriftFriction * Time.deltaTime;
        localDriftVelocity.z += slowDownDir.y * driftFriction * Time.deltaTime;

        if (Mathf.Abs(localDriftVelocity.x) < 0.01f)
        {
            localDriftVelocity.x = 0;
        }
        if (Mathf.Abs(localDriftVelocity.z) < 0.01f)
        {
            localDriftVelocity.z = 0;
        }
        localVelocity = localDriftVelocity;
        worldDriftVelocity = transform.TransformVector(localDriftVelocity);
    }

    private void Steering()
    {
        int turnDir = 1;
        if (localVelocity.z < 0 && !isDrifting)
            turnDir = -1;

        float magnitude = rb.velocity.magnitude;
        float turningMultiplier = (magnitude > speedAtWhichTurningSlows) ? 1 : Mathf.Abs(magnitude / speedAtWhichTurningSlows);

        float turningValuePercent;

        if (!isUsingController)
        {
            // if player wants to turn left, but car is turning right, and vice versa

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
                }
                else
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

        float zRot = 30 * turningValuePercent;
        frontRightWheelTurnParent.localRotation = Quaternion.Euler(new Vector3(0, 0, zRot));
        //frontRightWheel.Rotate(Vector3.right * zRot, Space.Self);
        frontLeftWheelTurnParent.localRotation = Quaternion.Euler(new Vector3(0, 0, zRot));

        float thisTurnAmount = isDrifting ? driftingTurnAmount : turnAmount;
        if (!isInAir)
        {   
            localVelocity.z -= turningMultiplier * Mathf.Abs(turningValuePercent) * turnSlowDownAmount * Time.deltaTime;
            rb.MoveRotation(transform.rotation * Quaternion.AngleAxis(turningValuePercent * turnDir * thisTurnAmount * turningMultiplier * Time.deltaTime, transform.up));
        }
    }

    void SpinWheels()
    {
        Vector3 localRigidbodyVelocity = transform.InverseTransformVector(rb.velocity);
        if (!isInAir)
        {
            float frontWheelRotAmount = 0;
           

            // change this so that the back wheels rotate when drifting, but the front wheels rotate based on the velocity
            if (gasInput != 0)
            {
                if (!isDrifting)
                {
                    frontWheelRotAmount = localRigidbodyVelocity.z * tireSpinAmount * Time.deltaTime;
                }
                else
                {
                    frontWheelRotAmount = gasInput * driftTireSpinAmount * Time.deltaTime;
                }
            } 
            else if (brakeInput != 0)
            {
                if (!isDrifting)
                {
                    frontWheelRotAmount = -localRigidbodyVelocity.z * tireSpinAmount * Time.deltaTime;
                }
                else
                {
                    frontWheelRotAmount = -brakeInput * driftTireSpinAmount * Time.deltaTime;
                }
            } else if (localRigidbodyVelocity != Vector3.zero)
            {
                if (isDrifting)
                {
                    if (localRigidbodyVelocity.z > speedWhereWheelsStartSpinningWhenDrifting)
                    {
                        frontWheelRotAmount = (localRigidbodyVelocity.z - speedWhereWheelsStartSpinningWhenDrifting) * driftTireSpinAmount / 10 * Time.deltaTime;
                    }
                } else
                {
                    frontWheelRotAmount = localRigidbodyVelocity.z * tireSpinAmount * Time.deltaTime;
                }
            }

            if (frontWheelRotAmount != 0)
            {
                backLeftWheel.Rotate(Vector3.up * frontWheelRotAmount, Space.Self);
                backRightWheel.Rotate(Vector3.up * frontWheelRotAmount, Space.Self);
                frontRightWheel.Rotate(Vector3.up * frontWheelRotAmount, Space.Self);
                frontLeftWheel.Rotate(Vector3.up * frontWheelRotAmount, Space.Self);
            }
        } else if (!wasInAirLastFrame)
        {
            // store the wheel spin speed
        } else
        {
            // slow down the wheel spinning when in the air to a complete stop
        }
    }

    #region Getters
    public bool GetIsInAir()
    {
        return isInAir;
    }

    public float GetSpeed()
    {
        return rb.velocity.magnitude;
    }

    public Vector3 GetWorldVelocity()
    {
        return rb.velocity;
    }

    public Vector3 GetLocalVelocity()
    {
        return transform.InverseTransformVector(rb.velocity);
    }

    public bool GetIsDrifting()
    {
        return isDrifting;
    }

    #endregion

    private void FixedUpdate()
    {
        // if in the air
        if (numTiresOnGround <= numWheelsOnGroundConsideredInAir)
        {
            if (timeSinceLeftAirTimer > timeInAirBeforeCanAirRoll)
            {
                carMesh.SetBlendShapeWeight(0, Mathf.Clamp(carMesh.GetBlendShapeWeight(0) + Time.deltaTime * 300, 0, 100));
                carMesh.SetBlendShapeWeight(2, Mathf.Clamp(carMesh.GetBlendShapeWeight(2) + Time.deltaTime * 300, 0, 100));
                isInAir = true;
                // if on keyboard and mouse, you can air roll without holding air roll button. On controller you have to hold air roll(drift) button to air roll.
                if ((!isUsingController || airRollIsPressed) && airRollInput != 0)
                {
                    rb.AddRelativeTorque(Vector3.forward * -airRollInput * sideRotationForce, ForceMode.Acceleration);
                }

                if (airRotInput != Vector2.zero)
                {
                    rb.AddRelativeTorque(Vector3.right * airRotInput.y * sideRotationForce, ForceMode.Acceleration);
                    rb.AddRelativeTorque(Vector3.up * airRotInput.x * sideRotationForce, ForceMode.Acceleration);
                }
            }          
        } else
        {
            carMesh.SetBlendShapeWeight(0, Mathf.Clamp(carMesh.GetBlendShapeWeight(0) - Time.deltaTime * 200, 0, 100));
            carMesh.SetBlendShapeWeight(2, Mathf.Clamp(carMesh.GetBlendShapeWeight(2) - Time.deltaTime * 200, 0, 100));
        }
    }
}
