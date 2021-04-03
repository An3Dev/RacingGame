using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarInput))]

public class PhysicsCarController : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] float forwardForce;
    [SerializeField] float maxForwardVelocity = 10;
    [SerializeField] float maxReverseVelocity = 10;

    [SerializeField] float accelerationMagnitude = 1;

    [Tooltip("The higher the friction, the faster the car stops when engine is idle.")]
    [SerializeField] float friction;
    [SerializeField] float brakeForce;

    [SerializeField] float turnAmount = 10;
    [SerializeField] float speedAtWhichTurningSlows = 3;
    [SerializeField] float maxTurnValue = 100;

    [Range(0, 49.99f)]
    [Tooltip("At 49, it takes the longest time to return to having the steering wheel straight. At 0, the steering wheel is straight instantly.")]
    [SerializeField] float straightenOutSteeringWheelMultiplier = 1;

    Rigidbody rb;

    [SerializeField] Transform[] groundCheck;
    [SerializeField] float groundCheckRadius = 0.1f;

    float currentTurnValue = 0;


    Vector3 localVelocity = Vector3.zero;
    Vector2 movementInput;

    public int tiresOnGround;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ReceiveMovementInput(Vector2 moveDir)
    {
        movementInput = moveDir;
    }

    private void FixedUpdate()
    {
        int turnDir = 1;
        //if (localVelocity.z < 0)
        //    turnDir = -1;

        tiresOnGround = 0;
        for (int i = 0; i < groundCheck.Length; i++)
        {
            Collider[] colliders = Physics.OverlapSphere(groundCheck[i].position, groundCheckRadius);
            foreach (Collider c in colliders)
            {
                if (!c.transform.root.Equals(transform))
                {
                    tiresOnGround++;
                    break;
                }
            }
        }

        if (tiresOnGround < 3)
        {
            // don't add velocity to the car

            // apply slow down to car in air

            // do not allow rotation

            Debug.Log("In air");

            return;
        }

        float turningMultiplier = (transform.InverseTransformDirection(rb.velocity.normalized).z > speedAtWhichTurningSlows) ? 1 : localVelocity.z / speedAtWhichTurningSlows;

        // if player wants to turn left, but car is turning right, and vice versa
        if ((movementInput.x > 0 && currentTurnValue < 0) || (movementInput.x < 0 && currentTurnValue > 0))
        {
            //currentTurnValue = 0;
            currentTurnValue += movementInput.x * Time.fixedDeltaTime * 100;
        }
        else
        {
            if (movementInput.x != 0)
            {
                currentTurnValue += movementInput.x * Time.fixedDeltaTime * 100;
            }
            else
            {
                float val = straightenOutSteeringWheelMultiplier * Time.fixedDeltaTime;
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

        transform.Rotate(new Vector3(0, currentTurnValue / maxTurnValue, 0) * turnDir * turnAmount * turningMultiplier * Time.fixedDeltaTime, Space.Self);


        rb.AddForce(transform.forward * movementInput.y * forwardForce * Time.fixedDeltaTime);

        // limit the velocity
        //if (localVelocity.z > maxForwardVelocity)
        //{
        //    localVelocity.z = maxForwardVelocity;
        //}
        //else if (localVelocity.z < -maxReverseVelocity)
        //{
        //    localVelocity.z = -maxReverseVelocity;
        //}
        //else
        //{
        //    // accelerates car  
        //    localVelocity += Vector3.forward * movementInput.y * accelerationMagnitude;
        //}



        // if there's no input, and the car is not moving downhill, slow down the car
        //if (movementInput.y == 0 && localVelocity.z != 0 && rb.velocity.y > -1)
        //{
        //    int slowDownDir = 1;
        //    if (localVelocity.z < 0)
        //    {
        //        slowDownDir = -1;
        //    }

        //    localVelocity.z -= slowDownDir * friction * Time.deltaTime;
        //    if (Mathf.Abs(localVelocity.z) < 0.01f)
        //    {
        //        localVelocity.z = 0;
        //    }
        //}

        //Vector3 worldVelocity = transform.TransformVector(localVelocity);
        //worldVelocity.y = rb.velocity.y;
        //rb.velocity = worldVelocity;



    }
}
